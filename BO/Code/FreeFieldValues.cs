namespace BO.Code
{
    /// <summary>
    /// Hodnoty uživatelských polí (x28EntityField):
    ///  - obor hodnot pole (x28DataSource) je seznam oddělený středníkem,
    ///  - pole s vícenásobným výběrem (x28IsMultiselect) ukládá vybrané hodnoty do jednoho textového sloupce oddělené znakem |.
    /// </summary>
    public static class FreeFieldValues
    {
        public const char MultiDelimiter = '|';
        public const char SourceDelimiter = ';';

        private static List<string> Split(string s, char[] delimiters)
        {
            var lis = new List<string>();
            if (string.IsNullOrWhiteSpace(s)) return lis;
            foreach (string part in s.Split(delimiters))
            {
                string val = part.Trim();
                if (val.Length > 0 && !lis.Contains(val)) lis.Add(val);
            }
            return lis;
        }

        /// <summary>Obor hodnot jako seznam: oddělovač středník nebo konec řádku, bez prázdných a opakovaných hodnot.</summary>
        public static List<string> ParseSource(string strDataSource)
        {
            return Split(strDataSource, new[] { SourceDelimiter, '\r', '\n' });
        }

        /// <summary>Obor hodnot ve tvaru pro uložení do x28DataSource (oddělovač středník); prázdný obor = null.</summary>
        public static string NormalizeSource(string strDataSource)
        {
            var lis = ParseSource(strDataSource);
            return lis.Count == 0 ? null : string.Join(SourceDelimiter.ToString(), lis);
        }

        /// <summary>Uložená hodnota pole s vícenásobným výběrem jako seznam hodnot.</summary>
        public static List<string> ParseMulti(string strValue)
        {
            return Split(strValue, new[] { MultiDelimiter });
        }

        /// <summary>Hodnota pole s vícenásobným výběrem ve tvaru pro uložení (hodnoty oddělené |); nic vybráno = null.</summary>
        public static string NormalizeMulti(string strValue)
        {
            var lis = ParseMulti(strValue);
            return lis.Count == 0 ? null : string.Join(MultiDelimiter.ToString(), lis);
        }

        /// <summary>Hodnota pole s vícenásobným výběrem pro zobrazení v textu: „A, B, C“.</summary>
        public static string DisplayMulti(string strValue)
        {
            var lis = ParseMulti(strValue);
            return lis.Count == 0 ? null : string.Join(", ", lis);
        }
    }
}
