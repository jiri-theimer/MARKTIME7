

namespace BO
{
    public abstract class BaseRecDisposition
    {
        public bool OwnerAccess { get; set; }       //oprávnění vlastníka k uloženému záznamu
        public bool ReadAccess { get; set; }        //oprávnění číst uložený záznam
        public int a55ID { get; set; }              //přidělená individuální stránka záznamu
        
    }
}
