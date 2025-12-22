using System.ComponentModel.DataAnnotations;

namespace BO
{
    public enum x58FormatFlagEnum
    {
        String = 1,
        Numeric2 = 2,
        Numeric0 = 3,
        Date=4
        
    }
    public class x58WidgetParam:BaseBO
    {
        [Key]
        public int x58ID { get; set; }
        public int x01ID { get; set; }
        public x58FormatFlagEnum x58FormatFlag { get; set; }
        public string x58Name { get; set; }
        public string x58LabelText { get; set; }
        public string x58Description { get; set; }
        public int x58Ordinal { get; set; }
        public bool x58IsAllowEmptyValue { get; set; }
        public string x58Datasource { get; set; }

        public string GetParam4DTName()
        {
            switch (this.x58FormatFlag)
            {
                case x58FormatFlagEnum.Numeric0:
                    return "int";
                case x58FormatFlagEnum.Numeric2:
                    return "double";
                case x58FormatFlagEnum.Date:
                    return "datetime";
                default:
                    return "string";
            }
        }

        public object GetParam4DTValue(string parvalue)
        {
            
            switch (this.x58FormatFlag)
            {
                case x58FormatFlagEnum.Numeric0:
                    return Code.Bas.InInt(parvalue);

                case x58FormatFlagEnum.Numeric2:
                    return BO.Code.Bas.InDouble(parvalue);
                case x58FormatFlagEnum.Date:
                    if (string.IsNullOrEmpty(parvalue))
                    {
                        return System.DBNull.Value;
                    }
                    return Code.Bas.String2Date(parvalue);
                default:
                    if (string.IsNullOrEmpty(parvalue))
                    {
                        return System.DBNull.Value;
                    }

                    return parvalue;
            }
        }
    }
}
