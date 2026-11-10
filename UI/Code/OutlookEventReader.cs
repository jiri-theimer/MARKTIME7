using System.Text;

namespace UI.Code
{
    /// <summary>
    /// Událost kalendáře načtená ze souboru přetaženého z Outlooku (.msg) nebo uloženého jako .ics.
    /// Časy jsou v místním čase serveru (stejně jako p31DateTimeFrom_Orig).
    /// </summary>
    public class CalEvent
    {
        public string Uid { get; set; }             // GlobalObjectId (.msg) / UID (.ics), hex nebo text
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Location { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public bool AllDay { get; set; }
        public bool Cancelled { get; set; }
        public bool Declined { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsFree { get; set; }             // zobrazeno jako volno
        public bool Recurring { get; set; }
        public string OrganizerName { get; set; }
        public string OrganizerEmail { get; set; }
        public List<CalPerson> Attendees { get; set; } = new List<CalPerson>();
        public string FileName { get; set; }
    }

    public class CalPerson
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PartStat { get; set; }    // .ics: ACCEPTED / DECLINED / TENTATIVE / NEEDS-ACTION
    }

    /// <summary>
    /// Čtení událostí bez externích knihoven: .msg (Outlook, IPM.Appointment / IPM.Schedule.Meeting.*) a .ics (VEVENT).
    /// .msg je Compound File (CFB) s vlastnostmi MAPI ([MS-OXMSG]); čteme jen hlavičku a text, přílohy ignorujeme.
    /// </summary>
    public static class OutlookEventReader
    {
        public const int MaxFileBytes = 15 * 1024 * 1024;

        public static List<CalEvent> Read(string fileName, byte[] data)
        {
            var ret = new List<CalEvent>();
            if (data == null || data.Length == 0 || data.Length > MaxFileBytes) return ret;
            string ext = Path.GetExtension(fileName ?? "").ToLowerInvariant();

            if (ext == ".msg" || IsCfb(data))
            {
                var ev = MsgAppointmentReader.Read(data);
                if (ev != null) { ev.FileName = fileName; ret.Add(ev); }
                return ret;
            }
            if (ext == ".ics" || ext == ".ical" || ext == ".vcs")
            {
                foreach (var ev in IcsReader.Read(DecodeText(data)))
                {
                    ev.FileName = fileName; ret.Add(ev);
                }
            }
            return ret;
        }

        public static bool IsCfb(byte[] d)
        {
            return d.Length >= 512 && d[0] == 0xD0 && d[1] == 0xCF && d[2] == 0x11 && d[3] == 0xE0 && d[4] == 0xA1 && d[5] == 0xB1 && d[6] == 0x1A && d[7] == 0xE1;
        }

        private static string DecodeText(byte[] d)
        {
            if (d.Length >= 3 && d[0] == 0xEF && d[1] == 0xBB && d[2] == 0xBF) return Encoding.UTF8.GetString(d, 3, d.Length - 3);
            if (d.Length >= 2 && d[0] == 0xFF && d[1] == 0xFE) return Encoding.Unicode.GetString(d, 2, d.Length - 2);
            return Encoding.UTF8.GetString(d);
        }

        /// <summary>Text těla bez balastu: odkazy na online schůzky, oddělovače, prázdné řádky.</summary>
        public static string CleanBody(string s, int maxlen = 1000)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Replace("\r\n", "\n").Replace('\r', '\n');
            var lines = new List<string>();
            foreach (var raw in s.Split('\n'))
            {
                string l = raw.Trim();
                if (l.StartsWith("____") || l.StartsWith("----") || l.StartsWith("....")) break;   // pod čarou bývá pozvánka do Teams/Zoom
                if (l.Contains("teams.microsoft.com") || l.Contains("zoom.us/") || l.StartsWith("Microsoft Teams", StringComparison.OrdinalIgnoreCase)) break;
                if (l.Length == 0 && (lines.Count == 0 || lines[lines.Count - 1].Length == 0)) continue;
                lines.Add(l);
            }
            s = string.Join("\n", lines).Trim();
            if (s.Length == 0) return null;
            return s.Length > maxlen ? s.Substring(0, maxlen) : s;
        }
    }


    /// <summary>Minimální čtečka Compound File Binary ([MS-CFB]) – jen čtení streamů podle cesty.</summary>
    internal class CfbFile
    {
        private readonly byte[] _d;
        private readonly int _secSize, _miniSecSize;
        private readonly uint _miniCutoff;
        private readonly List<uint> _fat = new List<uint>();
        private readonly List<uint> _miniFat = new List<uint>();
        private byte[] _miniStream;
        public readonly List<Entry> Entries = new List<Entry>();

        private const uint ENDOFCHAIN = 0xFFFFFFFE, FREESECT = 0xFFFFFFFF, NOSTREAM = 0xFFFFFFFF;

        public class Entry
        {
            public int Index;
            public string Name;
            public byte Type;      // 1 storage, 2 stream, 5 root
            public uint Left, Right, Child, Start;
            public ulong Size;
        }

        public CfbFile(byte[] data)
        {
            _d = data;
            int secShift = U16(0x1E);
            if (secShift != 9 && secShift != 12) throw new InvalidDataException("CFB: sector size");
            _secSize = 1 << secShift;
            _miniSecSize = 1 << U16(0x20);
            uint numFatSec = U32(0x2C);
            uint dirStart = U32(0x30);
            _miniCutoff = U32(0x38);
            uint miniFatStart = U32(0x3C);
            uint difatStart = U32(0x44);
            uint numDifat = U32(0x48);

            // DIFAT: 109 položek v hlavičce + řetěz DIFAT sektorů
            var fatSectors = new List<uint>();
            for (int i = 0; i < 109; i++)
            {
                uint s = U32(0x4C + i * 4);
                if (s != FREESECT && s != ENDOFCHAIN) fatSectors.Add(s);
            }
            uint ds = difatStart; int guard = 0;
            while (ds != ENDOFCHAIN && ds != FREESECT && numDifat > 0 && guard++ < 10000)
            {
                long off = SecOff(ds);
                int n = _secSize / 4 - 1;
                for (int i = 0; i < n; i++)
                {
                    uint s = U32(off + i * 4);
                    if (s != FREESECT && s != ENDOFCHAIN) fatSectors.Add(s);
                }
                ds = U32(off + n * 4);
            }
            foreach (uint fs in fatSectors.Take((int)Math.Max(numFatSec, (uint)fatSectors.Count)))
            {
                long off = SecOff(fs);
                for (int i = 0; i < _secSize / 4; i++) _fat.Add(U32(off + i * 4));
            }

            // adresář
            var dir = ReadChain(dirStart, -1);
            for (int i = 0; i * 128 + 128 <= dir.Length; i++)
            {
                int o = i * 128;
                int nameLen = BitConverter.ToUInt16(dir, o + 64);
                var e = new Entry
                {
                    Index = i,
                    Name = nameLen >= 2 ? Encoding.Unicode.GetString(dir, o, Math.Min(64, nameLen) - 2) : "",
                    Type = dir[o + 66],
                    Left = BitConverter.ToUInt32(dir, o + 68),
                    Right = BitConverter.ToUInt32(dir, o + 72),
                    Child = BitConverter.ToUInt32(dir, o + 76),
                    Start = BitConverter.ToUInt32(dir, o + 116),
                    Size = BitConverter.ToUInt64(dir, o + 120)
                };
                if (_secSize == 512) e.Size &= 0xFFFFFFFF;
                Entries.Add(e);
            }
            if (Entries.Count == 0 || Entries[0].Type != 5) throw new InvalidDataException("CFB: root");

            // mini FAT + mini stream (obsah root entry)
            if (miniFatStart != ENDOFCHAIN && miniFatStart != FREESECT)
            {
                var mf = ReadChain(miniFatStart, -1);
                for (int i = 0; i + 4 <= mf.Length; i += 4) _miniFat.Add(BitConverter.ToUInt32(mf, i));
            }
            _miniStream = Entries[0].Start == ENDOFCHAIN ? new byte[0] : ReadChain(Entries[0].Start, (long)Entries[0].Size);
        }

        /// <summary>Děti storage (procházení stromu sourozenců).</summary>
        public List<Entry> Children(Entry storage)
        {
            var ret = new List<Entry>();
            var stack = new Stack<uint>();
            if (storage.Child != NOSTREAM) stack.Push(storage.Child);
            int guard = 0;
            while (stack.Count > 0 && guard++ < 100000)
            {
                uint id = stack.Pop();
                if (id >= Entries.Count) continue;
                var e = Entries[(int)id];
                ret.Add(e);
                if (e.Left != NOSTREAM) stack.Push(e.Left);
                if (e.Right != NOSTREAM) stack.Push(e.Right);
            }
            return ret;
        }

        public Entry Child(Entry storage, string name)
        {
            return Children(storage).FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public byte[] ReadStream(Entry e)
        {
            if (e == null || e.Type != 2) return null;
            if (e.Size == 0) return new byte[0];
            if (e.Size < _miniCutoff)
            {
                var ms = new MemoryStream();
                uint s = e.Start; int guard = 0;
                while (s != ENDOFCHAIN && s < _miniFat.Count + 1 && guard++ < 1000000 && (ulong)ms.Length < e.Size)
                {
                    long off = (long)s * _miniSecSize;
                    if (off + _miniSecSize > _miniStream.Length) break;
                    ms.Write(_miniStream, (int)off, _miniSecSize);
                    if (s >= _miniFat.Count) break;
                    s = _miniFat[(int)s];
                }
                var b = ms.ToArray();
                return b.Length > (int)e.Size ? b.Take((int)e.Size).ToArray() : b;
            }
            return ReadChain(e.Start, (long)e.Size);
        }

        private byte[] ReadChain(uint start, long size)
        {
            var ms = new MemoryStream();
            uint s = start; int guard = 0;
            while (s != ENDOFCHAIN && s != FREESECT && guard++ < 1000000)
            {
                long off = SecOff(s);
                if (off + _secSize > _d.Length) break;
                ms.Write(_d, (int)off, _secSize);
                if (size >= 0 && ms.Length >= size) break;
                if (s >= _fat.Count) break;
                s = _fat[(int)s];
            }
            var b = ms.ToArray();
            return (size >= 0 && b.Length > size) ? b.Take((int)size).ToArray() : b;
        }

        private long SecOff(uint s) => (long)(s + 1) * _secSize;
        private int U16(long o) => BitConverter.ToUInt16(_d, (int)o);
        private uint U32(long o) => BitConverter.ToUInt32(_d, (int)o);
    }


    /// <summary>Událost z Outlook .msg ([MS-OXMSG], [MS-OXOCAL]).</summary>
    internal static class MsgAppointmentReader
    {
        private static readonly Guid PSETID_Appointment = new Guid("00062002-0000-0000-C000-000000000046");
        private static readonly Guid PSETID_Meeting = new Guid("6ED8DA90-450B-101B-98DA-00AA003F1305");

        private class Props
        {
            public Dictionary<ushort, byte[]> Fixed = new Dictionary<ushort, byte[]>();   // id → 8 bajtů hodnoty
            public Dictionary<ushort, ushort> FixedType = new Dictionary<ushort, ushort>();
            public CfbFile Cfb; public CfbFile.Entry Storage; public List<CfbFile.Entry> Kids;
            public Encoding Enc8 = Win1252();

            public string Str(ushort id)
            {
                var e = Kids.FirstOrDefault(p => p.Name.Equals($"__substg1.0_{id:X4}001F", StringComparison.OrdinalIgnoreCase));
                if (e != null) return TrimZ(Encoding.Unicode.GetString(Cfb.ReadStream(e)));
                e = Kids.FirstOrDefault(p => p.Name.Equals($"__substg1.0_{id:X4}001E", StringComparison.OrdinalIgnoreCase));
                if (e != null) return TrimZ(Enc8.GetString(Cfb.ReadStream(e)));
                return null;
            }
            public byte[] Bin(ushort id)
            {
                var e = Kids.FirstOrDefault(p => p.Name.Equals($"__substg1.0_{id:X4}0102", StringComparison.OrdinalIgnoreCase));
                return e == null ? null : Cfb.ReadStream(e);
            }
            public DateTime? Time(ushort id)
            {
                if (!Fixed.TryGetValue(id, out var v) || FixedType[id] != 0x0040) return null;
                long ft = BitConverter.ToInt64(v, 0);
                if (ft <= 0 || ft >= DateTime.MaxValue.ToFileTimeUtc()) return null;
                try { return DateTime.FromFileTimeUtc(ft); } catch { return null; }
            }
            public int? Int(ushort id)
            {
                if (!Fixed.TryGetValue(id, out var v)) return null;
                return BitConverter.ToInt32(v, 0);
            }
            public bool? Bool(ushort id)
            {
                if (!Fixed.TryGetValue(id, out var v)) return null;
                return v[0] != 0;
            }
            private static string TrimZ(string s) => s?.TrimEnd('\0');
        }

        private static Encoding Win1252()
        {
            try { Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); return Encoding.GetEncoding(1252); } catch { return Encoding.Latin1; }
        }

        private static Props LoadProps(CfbFile cfb, CfbFile.Entry storage, int headerSize)
        {
            var p = new Props { Cfb = cfb, Storage = storage, Kids = cfb.Children(storage) };
            var ps = p.Kids.FirstOrDefault(e => e.Name == "__properties_version1.0");
            var b = cfb.ReadStream(ps);
            if (b != null)
            {
                for (int o = headerSize; o + 16 <= b.Length; o += 16)
                {
                    ushort type = BitConverter.ToUInt16(b, o);
                    ushort id = BitConverter.ToUInt16(b, o + 2);
                    if ((type & 0x1000) != 0) continue;   // multivalue
                    if (type == 0x001F || type == 0x001E || type == 0x0102 || type == 0x000D || type == 0x0048) continue;   // proměnná délka → v __substg
                    p.Fixed[id] = b.Skip(o + 8).Take(8).ToArray();
                    p.FixedType[id] = type;
                }
            }
            return p;
        }

        public static CalEvent Read(byte[] data)
        {
            CfbFile cfb;
            try { cfb = new CfbFile(data); } catch { return null; }
            var root = cfb.Entries[0];
            var m = LoadProps(cfb, root, 32);

            int? cp = m.Int(0x3FFD) ?? m.Int(0x3FDE);   // PidTagMessageCodepage / PidTagInternetCodepage
            if (cp > 0)
            {
                try { m.Enc8 = Encoding.GetEncoding(cp.Value); } catch { }
            }

            string msgClass = m.Str(0x001A) ?? "";
            bool isAppt = msgClass.StartsWith("IPM.Appointment", StringComparison.OrdinalIgnoreCase) || msgClass.StartsWith("IPM.Schedule.Meeting", StringComparison.OrdinalIgnoreCase);
            if (!isAppt) return null;

            var named = MapNamed(cfb, root);
            ushort N(Guid g, uint lid) => named.TryGetValue((g, lid), out var id) ? id : (ushort)0;

            var ev = new CalEvent
            {
                Subject = m.Str(0x0037),
                Body = m.Str(0x1000),
                Location = Nz(m.Str(N(PSETID_Appointment, 0x8208)))
            };
            if (string.IsNullOrWhiteSpace(ev.Body))
            {
                var html = m.Bin(0x1013);
                if (html != null) ev.Body = HtmlToText(Encoding.UTF8.GetString(html));
            }

            DateTime? s = m.Time(N(PSETID_Appointment, 0x820D)) ?? m.Time(0x0060);   // AppointmentStartWhole / PidTagStartDate
            DateTime? e = m.Time(N(PSETID_Appointment, 0x820E)) ?? m.Time(0x0061);
            ev.Start = s?.ToLocalTime(); ev.End = e?.ToLocalTime();

            ev.AllDay = m.Bool(N(PSETID_Appointment, 0x8215)) == true;               // AppointmentSubType
            ev.Recurring = m.Bool(N(PSETID_Appointment, 0x8223)) == true;
            int? busy = m.Int(N(PSETID_Appointment, 0x8205));                         // BusyStatus 0 = volno
            ev.IsFree = busy == 0;
            int? state = m.Int(N(PSETID_Appointment, 0x8217));                        // AppointmentStateFlags, 0x4 = zrušeno
            ev.Cancelled = (state.HasValue && (state.Value & 0x4) != 0) || msgClass.StartsWith("IPM.Schedule.Meeting.Canceled", StringComparison.OrdinalIgnoreCase);
            int? resp = m.Int(N(PSETID_Appointment, 0x8218));                         // ResponseStatus 4 = odmítnuto
            ev.Declined = resp == 4 || msgClass.StartsWith("IPM.Schedule.Meeting.Resp.Neg", StringComparison.OrdinalIgnoreCase);
            ev.IsPrivate = m.Int(0x0036) == 2;                                        // PidTagSensitivity

            var gid = m.Bin(N(PSETID_Meeting, 0x0023)) ?? m.Bin(N(PSETID_Meeting, 0x0003));   // CleanGlobalObjectId / GlobalObjectId
            if (gid != null && gid.Length > 0) ev.Uid = Convert.ToHexString(gid);
            if (ev.Uid == null)
            {
                var key = m.Bin(0x0FF9) ?? m.Bin(0x300B);   // PidTagRecordKey / PidTagSearchKey
                if (key != null && key.Length > 0) ev.Uid = Convert.ToHexString(key);
            }

            ev.OrganizerName = Nz(m.Str(0x0042)) ?? Nz(m.Str(0x0C1A));                // SentRepresentingName / SenderName
            ev.OrganizerEmail = Mail(m.Str(0x5D02)) ?? Mail(m.Str(0x0065)) ?? Mail(m.Str(0x5D01)) ?? Mail(m.Str(0x0C1F));

            foreach (var rs in m.Kids.Where(k => k.Type == 1 && k.Name.StartsWith("__recip_version1.0_", StringComparison.OrdinalIgnoreCase)).OrderBy(k => k.Name))
            {
                var r = LoadProps(cfb, rs, 8);
                r.Enc8 = m.Enc8;
                string email = Mail(r.Str(0x39FE)) ?? Mail(r.Str(0x3003)) ?? Mail(r.Str(0x5FF6));
                string name = Nz(r.Str(0x3001)) ?? Nz(r.Str(0x5FF6));
                if (email == null && name == null) continue;
                ev.Attendees.Add(new CalPerson { Name = name, Email = email });
            }
            return ev;
        }

        /// <summary>Mapa pojmenovaných vlastností (GUID, LID) → ID 0x8000+ ze storage __nameid_version1.0.</summary>
        private static Dictionary<(Guid, uint), ushort> MapNamed(CfbFile cfb, CfbFile.Entry root)
        {
            var ret = new Dictionary<(Guid, uint), ushort>();
            var ns = cfb.Child(root, "__nameid_version1.0");
            if (ns == null) return ret;
            var guids = cfb.ReadStream(cfb.Child(ns, "__substg1.0_00020102")) ?? new byte[0];
            var ents = cfb.ReadStream(cfb.Child(ns, "__substg1.0_00030102")) ?? new byte[0];
            for (int o = 0; o + 8 <= ents.Length; o += 8)
            {
                uint nameId = BitConverter.ToUInt32(ents, o);
                uint ik = BitConverter.ToUInt32(ents, o + 4);
                if ((ik & 1) != 0) continue;   // pojmenované řetězcem – nepotřebujeme
                int gi = (int)((ik >> 1) & 0x7FFF);
                ushort propIndex = (ushort)(ik >> 16);
                Guid g;
                if (gi == 1) g = new Guid("00020328-0000-0000-C000-000000000046");        // PS_MAPI
                else if (gi == 2) g = new Guid("00020329-0000-0000-C000-000000000046");   // PS_PUBLIC_STRINGS
                else
                {
                    int go = (gi - 3) * 16;
                    if (go < 0 || go + 16 > guids.Length) continue;
                    g = new Guid(guids.Skip(go).Take(16).ToArray());
                }
                ret[(g, nameId)] = (ushort)(0x8000 + propIndex);
            }
            return ret;
        }

        private static string Nz(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        private static string Mail(string s)
        {
            s = Nz(s);
            if (s == null || !s.Contains('@') || s.StartsWith("/")) return null;   // /O=EXCHANGE… není SMTP
            if (s.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)) s = s.Substring(7);
            return s.ToLowerInvariant();
        }

        internal static string HtmlToText(string html)
        {
            if (string.IsNullOrEmpty(html)) return null;
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            foreach (var n in doc.DocumentNode.SelectNodes("//script|//style|//head") ?? Enumerable.Empty<HtmlAgilityPack.HtmlNode>()) n.Remove();
            var sb = new StringBuilder();
            foreach (var n in doc.DocumentNode.Descendants())
            {
                if (n.NodeType == HtmlAgilityPack.HtmlNodeType.Text) sb.Append(System.Net.WebUtility.HtmlDecode(n.InnerText));
                else if (n.Name == "br" || n.Name == "p" || n.Name == "div" || n.Name == "tr" || n.Name == "li") sb.Append('\n');
            }
            return sb.ToString();
        }
    }


    /// <summary>Čtení VEVENT z iCalendar (RFC 5545). Opakované události: bere se první výskyt (DTSTART), Recurring=true.</summary>
    internal static class IcsReader
    {
        public static List<CalEvent> Read(string text)
        {
            var ret = new List<CalEvent>();
            if (string.IsNullOrEmpty(text)) return ret;
            // rozbalení zalomených řádků
            text = text.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n ", "").Replace("\n\t", "");
            CalEvent ev = null; int depth = 0;
            foreach (var line in text.Split('\n'))
            {
                if (line.Length == 0) continue;
                int colon = IndexOfValueColon(line);
                if (colon < 0) continue;
                string head = line.Substring(0, colon), val = line.Substring(colon + 1);
                var parts = head.Split(';');
                string name = parts[0].ToUpperInvariant();
                var pars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in parts.Skip(1))
                {
                    int eq = p.IndexOf('=');
                    if (eq > 0) pars[p.Substring(0, eq)] = p.Substring(eq + 1).Trim('"');
                }

                if (name == "BEGIN")
                {
                    if (val.Equals("VEVENT", StringComparison.OrdinalIgnoreCase) && ev == null) { ev = new CalEvent(); depth = 0; }
                    else if (ev != null) depth++;   // VALARM apod.
                    continue;
                }
                if (name == "END")
                {
                    if (ev != null && depth > 0) { depth--; continue; }
                    if (ev != null && val.Equals("VEVENT", StringComparison.OrdinalIgnoreCase)) { ret.Add(ev); ev = null; }
                    continue;
                }
                if (ev == null || depth > 0) continue;

                switch (name)
                {
                    case "UID": ev.Uid = val.Trim(); break;
                    case "SUMMARY": ev.Subject = Unescape(val); break;
                    case "DESCRIPTION": ev.Body = Unescape(val); break;
                    case "LOCATION": ev.Location = Unescape(val); break;
                    case "STATUS": ev.Cancelled = val.Trim().Equals("CANCELLED", StringComparison.OrdinalIgnoreCase); break;
                    case "CLASS": ev.IsPrivate = val.Trim().Equals("PRIVATE", StringComparison.OrdinalIgnoreCase) || val.Trim().Equals("CONFIDENTIAL", StringComparison.OrdinalIgnoreCase); break;
                    case "TRANSP": ev.IsFree = val.Trim().Equals("TRANSPARENT", StringComparison.OrdinalIgnoreCase); break;
                    case "RRULE": ev.Recurring = true; break;
                    case "X-MICROSOFT-CDO-BUSYSTATUS": if (val.Trim().Equals("FREE", StringComparison.OrdinalIgnoreCase)) ev.IsFree = true; break;
                    case "DTSTART":
                        ev.Start = ParseDate(val, pars, out bool allday);
                        if (allday) ev.AllDay = true;
                        break;
                    case "DTEND": ev.End = ParseDate(val, pars, out _); break;
                    case "DURATION":
                        if (ev.Start != null && ev.End == null) { var d = ParseDuration(val); if (d != null) ev.End = ev.Start.Value.Add(d.Value); }
                        break;
                    case "ORGANIZER":
                        ev.OrganizerEmail = Mail(val);
                        ev.OrganizerName = pars.TryGetValue("CN", out var cn) ? cn : null;
                        break;
                    case "ATTENDEE":
                        var a = new CalPerson { Email = Mail(val), Name = pars.TryGetValue("CN", out var acn) ? acn : null, PartStat = pars.TryGetValue("PARTSTAT", out var ps) ? ps.ToUpperInvariant() : null };
                        if (a.Email != null || a.Name != null) ev.Attendees.Add(a);
                        break;
                }
            }
            foreach (var e in ret)
            {
                if (e.End == null && e.Start != null) e.End = e.AllDay ? e.Start.Value.AddDays(1) : e.Start;
                if (e.Start != null && e.End != null && e.End < e.Start) e.End = e.Start;
            }
            return ret;
        }

        // dvojtečka mimo uvozovky v parametrech (např. ATTENDEE;CN="X: Y":mailto:...)
        private static int IndexOfValueColon(string line)
        {
            bool q = false;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"') q = !q;
                else if (line[i] == ':' && !q) return i;
            }
            return -1;
        }

        private static string Unescape(string s)
        {
            if (s == null) return null;
            return s.Replace("\\n", "\n").Replace("\\N", "\n").Replace("\\,", ",").Replace("\\;", ";").Replace("\\\\", "\\").Trim();
        }

        private static string Mail(string v)
        {
            v = v?.Trim();
            if (string.IsNullOrEmpty(v)) return null;
            if (v.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)) v = v.Substring(7);
            return v.Contains('@') ? v.ToLowerInvariant() : null;
        }

        private static DateTime? ParseDate(string val, Dictionary<string, string> pars, out bool allday)
        {
            allday = false;
            val = val.Trim();
            if (val.Length == 8 || (pars.TryGetValue("VALUE", out var vt) && vt.Equals("DATE", StringComparison.OrdinalIgnoreCase)))
            {
                allday = true;
                if (DateTime.TryParseExact(val.Substring(0, 8), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var dd)) return dd;
                return null;
            }
            bool utc = val.EndsWith("Z");
            string core = val.TrimEnd('Z');
            if (!DateTime.TryParseExact(core, new[] { "yyyyMMdd'T'HHmmss", "yyyyMMdd'T'HHmm" }, null, System.Globalization.DateTimeStyles.None, out var d)) return null;
            if (utc) return DateTime.SpecifyKind(d, DateTimeKind.Utc).ToLocalTime();
            if (pars.TryGetValue("TZID", out var tzid))
            {
                var tz = FindTz(tzid);
                if (tz != null)
                {
                    try { return TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(d, DateTimeKind.Unspecified), tz, TimeZoneInfo.Local); } catch { }
                }
            }
            return d;   // floating time = místní
        }

        private static TimeZoneInfo FindTz(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            id = id.Trim().Trim('/');
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { }   // Windows i IANA (.NET 6+)
            // Outlook občas posílá zobrazované jméno, např. "(UTC+01:00) Amsterdam, Berlin, ..."
            var m = System.Text.RegularExpressions.Regex.Match(id, @"UTC([+-]\d\d):(\d\d)");
            if (m.Success)
            {
                int h = int.Parse(m.Groups[1].Value), min = int.Parse(m.Groups[2].Value);
                var off = new TimeSpan(h, h < 0 ? -min : min, 0);
                var z = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(t => t.DisplayName.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (z != null) return z;
                if (off == TimeZoneInfo.Local.BaseUtcOffset) return TimeZoneInfo.Local;   // stejné pásmo jako server
            }
            return null;
        }

        private static TimeSpan? ParseDuration(string v)
        {
            try { return System.Xml.XmlConvert.ToTimeSpan(v.Trim()); } catch { return null; }
        }
    }
}
