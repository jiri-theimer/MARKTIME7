namespace BO
{
    /// <summary>Položka check-listu v úkolu (tabulka p65Checklist).</summary>
    public class p65Checklist : BaseBO
    {
        public int p56ID { get; set; }
        public string p65Name { get; set; }
        public int p65Ordinary { get; set; }
        public int j02ID_Done { get; set; }

        public string DoneBy { get; }   //jméno toho, kdo položku odškrtl

        //Hotová položka = uzavřený záznam (isclosed, tedy ValidUntil už nastal); ValidUntil je pak čas odškrtnutí.
        public bool IsDone { get { return this.isclosed; } }
        public DateTime? DateDone { get { return this.isclosed ? this.ValidUntil : null; } }
    }
}
