namespace BO
{
    /// <summary>
    /// Položka check-listu (tabulka p65Checklist). Patří úkolu (p56ID), dokumentu (o23ID), nebo opakovanému úkolu jako vzor (p58ID).
    /// </summary>
    public class p65Checklist : BaseBO
    {
        public int p56ID { get; set; }
        public int p58ID { get; set; }
        public int o23ID { get; set; }
        public string p65Name { get; set; }
        public int p65Ordinary { get; set; }
        public int j02ID_Done { get; set; }

        public string DoneBy { get; }   //jméno toho, kdo položku odškrtl

        //Hotová položka = uzavřený záznam (isclosed, tedy ValidUntil už nastal); ValidUntil je pak čas odškrtnutí.
        public bool IsDone { get { return this.isclosed; } }
        public DateTime? DateDone { get { return this.isclosed ? this.ValidUntil : null; } }

        //komu položka patří: "p56" (úkol), "o23" (dokument) nebo "p58" (opakovaný úkol)
        public string OwnerPrefix { get { return this.p58ID > 0 ? "p58" : (this.o23ID > 0 ? "o23" : "p56"); } }
        public int OwnerPid { get { return this.p58ID > 0 ? this.p58ID : (this.o23ID > 0 ? this.o23ID : this.p56ID); } }
    }
}
