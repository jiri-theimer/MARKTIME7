

namespace BO
{
    public enum o27CallerFlagENUM
    {
        _None = 0,
        Notepad = 1
    }
    public class o27Attachment : BaseBO
    {

        public int x01ID { get; set; }

        public string o27Entity { get; set; }
        public int o27RecordPid { get; set; }
        public string o27Name { get; set; }
        public string o27OriginalFileName { get; set; }
        public string o27FileExtension { get; set; }
        public string o27ArchiveFileName { get; set; }
        public string o27ArchiveFolder { get; set; }
        public string o27WwwRootFolder { get; set; }
        public int o27FileSize { get; set; }
        public string o27ContentType { get; set; }
        public string o27FullText { get; set; }
        public Guid o27Guid { get; set; }

        public string o27NotepadTempGuid { get; set; }
        public o27CallerFlagENUM o27CallerFlag { get; set; }
        public string FullPath { get; set; } //pracovní

    }
}
