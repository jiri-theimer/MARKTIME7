namespace BO.Banka.Fio
{
    public class Rootobject
    {
        public Accountstatement accountStatement { get; set; }
    }

    public class Accountstatement
    {
        public Info info { get; set; }
        public Transactionlist transactionList { get; set; }
    }

    public class Info
    {
        public string accountId { get; set; }
        public string bankId { get; set; }
        public string currency { get; set; }
        public string iban { get; set; }
        public string bic { get; set; }
        public float openingBalance { get; set; }
        public float closingBalance { get; set; }
        public string dateStart { get; set; }
        public string dateEnd { get; set; }
        public object yearList { get; set; }
        public object idList { get; set; }
        public long idFrom { get; set; }
        public long idTo { get; set; }
        public object idLastDownload { get; set; }
    }

    public class Transactionlist
    {
        public Transaction[] transaction { get; set; }
    }

    public class Transaction
    {
        public Column22 column22 { get; set; }
        public Column0 column0 { get; set; }
        public Column1 column1 { get; set; }
        public Column14 column14 { get; set; }
        public Column2 column2 { get; set; }
        public Column10 column10 { get; set; }
        public Column3 column3 { get; set; }
        public Column12 column12 { get; set; }
        public Column4 column4 { get; set; }
        public Column5 column5 { get; set; }
        public Column6 column6 { get; set; }
        public Column7 column7 { get; set; }
        public Column16 column16 { get; set; }
        public Column8 column8 { get; set; }
        public Column9 column9 { get; set; }
        public object column18 { get; set; }
        public Column25 column25 { get; set; }
        public object column26 { get; set; }
        public Column17 column17 { get; set; }
        public object column27 { get; set; }
    }

    public class Column22
    {
        public long value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column0
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column1
    {
        public float value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column14
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column2
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column10
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column3
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column12
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column4
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column5
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column6
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column7
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column16
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column8
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column9
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column25
    {
        public string value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Column17
    {
        public long value { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }


}
