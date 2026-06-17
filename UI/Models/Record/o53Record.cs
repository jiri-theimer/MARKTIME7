

namespace UI.Models.Record
{
    public class o53Record:BaseRecordViewModel
    {
        public BO.o53TagGroup Rec { get; set; }

        public bool IsAllEntities { get; set; }
        public List<BO.TheEntity> ApplicableEntities { get; set; }
        

    }
}
