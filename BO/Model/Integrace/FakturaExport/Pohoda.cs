

namespace BO.Model.Integrace.FakturaExport
{
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/data.xsd")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.stormware.cz/schema/version_2/data.xsd", IsNullable = false)]
    public partial class dataPack
    {

        private dataPackDataPackItem[] dataPackItemField;

        private string versionField;

        private string idField;

        private string icoField;

        private string keyField;

        private string programVersionField;

        private string applicationField;

        private string noteField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("dataPackItem")]
        public dataPackDataPackItem[] dataPackItem
        {
            get
            {
                return dataPackItemField;
            }
            set
            {
                dataPackItemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string version
        {
            get
            {
                return versionField;
            }
            set
            {
                versionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string ico
        {
            get
            {
                return icoField;
            }
            set
            {
                icoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string key
        {
            get
            {
                return keyField;
            }
            set
            {
                keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string programVersion
        {
            get
            {
                return programVersionField;
            }
            set
            {
                programVersionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string application
        {
            get
            {
                return applicationField;
            }
            set
            {
                applicationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string note
        {
            get
            {
                return noteField;
            }
            set
            {
                noteField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/data.xsd")]
    public partial class dataPackDataPackItem
    {

        private invoice invoiceField;

        private string versionField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public invoice invoice
        {
            get
            {
                return invoiceField;
            }
            set
            {
                invoiceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string version
        {
            get
            {
                return versionField;
            }
            set
            {
                versionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd", IsNullable = false)]
    public partial class invoice
    {

        private invoiceInvoiceHeader invoiceHeaderField;

        private invoiceInvoiceItem[] invoiceDetailField;

        private invoiceInvoiceSummary invoiceSummaryField;

        private string versionField;

        /// <remarks/>
        public invoiceInvoiceHeader invoiceHeader
        {
            get
            {
                return invoiceHeaderField;
            }
            set
            {
                invoiceHeaderField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("invoiceItem", IsNullable = false)]
        public invoiceInvoiceItem[] invoiceDetail
        {
            get
            {
                return invoiceDetailField;
            }
            set
            {
                invoiceDetailField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceSummary invoiceSummary
        {
            get
            {
                return invoiceSummaryField;
            }
            set
            {
                invoiceSummaryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string version
        {
            get
            {
                return versionField;
            }
            set
            {
                versionField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeader
    {

        private string invoiceTypeField;

        private invoiceInvoiceHeaderNumber numberField;

        private string symVarField;

        private DateTime dateField;

        private DateTime dateTaxField;

        private DateTime dateAccountingField;

        private DateTime dateDueField;

        private invoiceInvoiceHeaderAccounting accountingField;

        private invoiceInvoiceHeaderClassificationVAT classificationVATField;

        private string textField;

        private invoiceInvoiceHeaderPartnerIdentity partnerIdentityField;

        private invoiceInvoiceHeaderMyIdentity myIdentityField;

        private invoiceInvoiceHeaderPaymentType paymentTypeField;

        private invoiceInvoiceHeaderAccount accountField;

        private string symConstField;

        private invoiceInvoiceHeaderCentre centreField;
        private string intNoteField;
        private string NoteField;

        private invoiceInvoiceHeaderContract contractField;

        private invoiceInvoiceHeaderLiquidation liquidationField;

        private bool markRecordField;

        /// <remarks/>
        public string invoiceType
        {
            get
            {
                return invoiceTypeField;
            }
            set
            {
                invoiceTypeField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderNumber number
        {
            get
            {
                return numberField;
            }
            set
            {
                numberField = value;
            }
        }

        /// <remarks/>
        public string symVar
        {
            get
            {
                return symVarField;
            }
            set
            {
                symVarField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public DateTime date
        {
            get
            {
                return dateField;
            }
            set
            {
                dateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public DateTime dateTax
        {
            get
            {
                return dateTaxField;
            }
            set
            {
                dateTaxField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public DateTime dateAccounting
        {
            get
            {
                return dateAccountingField;
            }
            set
            {
                dateAccountingField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public DateTime dateDue
        {
            get
            {
                return dateDueField;
            }
            set
            {
                dateDueField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderAccounting accounting
        {
            get
            {
                return accountingField;
            }
            set
            {
                accountingField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderClassificationVAT classificationVAT
        {
            get
            {
                return classificationVATField;
            }
            set
            {
                classificationVATField = value;
            }
        }

        /// <remarks/>
        public string text
        {
            get
            {
                return textField;
            }
            set
            {
                textField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderPartnerIdentity partnerIdentity
        {
            get
            {
                return partnerIdentityField;
            }
            set
            {
                partnerIdentityField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderMyIdentity myIdentity
        {
            get
            {
                return myIdentityField;
            }
            set
            {
                myIdentityField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderPaymentType paymentType
        {
            get
            {
                return paymentTypeField;
            }
            set
            {
                paymentTypeField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderAccount account
        {
            get
            {
                return accountField;
            }
            set
            {
                accountField = value;
            }
        }

        /// <remarks/>
        public string symConst
        {
            get
            {
                return symConstField;
            }
            set
            {
                symConstField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderContract contract
        {
            get
            {
                return contractField;
            }
            set
            {
                contractField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderLiquidation liquidation
        {
            get
            {
                return liquidationField;
            }
            set
            {
                liquidationField = value;
            }
        }

        /// <remarks/>
        public bool markRecord
        {
            get
            {
                return markRecordField;
            }
            set
            {
                markRecordField = value;
            }
        }

        public invoiceInvoiceHeaderCentre centre
        {
            get
            {
                return centreField;
            }
            set
            {
                centreField = value;
            }
        }

        public string intNote
        {
            get
            {
                return intNoteField;
            }
            set
            {
                intNoteField = value;
            }
        }

        public string note
        {
            get
            {
                return NoteField;
            }
            set
            {
                NoteField = value;
            }
        }


    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderNumber
    {

        private string numberRequestedField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string numberRequested
        {
            get
            {
                return numberRequestedField;
            }
            set
            {
                numberRequestedField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderAccounting
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return idsField;
            }
            set
            {
                idsField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderClassificationVAT
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return idsField;
            }
            set
            {
                idsField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderPartnerIdentity
    {

        private address addressField;

        private shipToAddress shipToAddressField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public address address
        {
            get
            {
                return addressField;
            }
            set
            {
                addressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public shipToAddress shipToAddress
        {
            get
            {
                return shipToAddressField;
            }
            set
            {
                shipToAddressField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd", IsNullable = false)]
    public partial class address
    {

        private string companyField;

        private string surnameField;

        private string nameField;

        private string cityField;

        private string streetField;

        private string numberField;

        private string zipField;

        private string icoField;

        private string dicField;

        /// <remarks/>
        public string company
        {
            get
            {
                return companyField;
            }
            set
            {
                companyField = value;
            }
        }

        /// <remarks/>
        public string surname
        {
            get
            {
                return surnameField;
            }
            set
            {
                surnameField = value;
            }
        }

        /// <remarks/>
        public string name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        public string city
        {
            get
            {
                return cityField;
            }
            set
            {
                cityField = value;
            }
        }

        /// <remarks/>
        public string street
        {
            get
            {
                return streetField;
            }
            set
            {
                streetField = value;
            }
        }

        /// <remarks/>
        public string number
        {
            get
            {
                return numberField;
            }
            set
            {
                numberField = value;
            }
        }

        /// <remarks/>
        public string zip
        {
            get
            {
                return zipField;
            }
            set
            {
                zipField = value;
            }
        }

        /// <remarks/>
        public string ico
        {
            get
            {
                return icoField;
            }
            set
            {
                icoField = value;
            }
        }

        /// <remarks/>
        public string dic
        {
            get
            {
                return dicField;
            }
            set
            {
                dicField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd", IsNullable = false)]
    public partial class shipToAddress
    {

        private object companyField;

        private object cityField;

        private object streetField;

        /// <remarks/>
        public object company
        {
            get
            {
                return companyField;
            }
            set
            {
                companyField = value;
            }
        }

        /// <remarks/>
        public object city
        {
            get
            {
                return cityField;
            }
            set
            {
                cityField = value;
            }
        }

        /// <remarks/>
        public object street
        {
            get
            {
                return streetField;
            }
            set
            {
                streetField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderMyIdentity
    {

        private address addressField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public address address
        {
            get
            {
                return addressField;
            }
            set
            {
                addressField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderPaymentType
    {

        private string idsField;

        private string paymentTypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return idsField;
            }
            set
            {
                idsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string paymentType
        {
            get
            {
                return paymentTypeField;
            }
            set
            {
                paymentTypeField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderAccount
    {

        private string idsField;

        private string accountNoField;
        private string bankCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return idsField;
            }
            set
            {
                idsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string accountNo
        {
            get
            {
                return accountNoField;
            }
            set
            {
                accountNoField = value;
            }
        }
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string bankCode
        {
            get
            {
                return bankCodeField;
            }
            set
            {
                bankCodeField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderContract
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return idsField;
            }
            set
            {
                idsField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderLiquidation
    {

        private decimal amountHomeField;

        private byte amountForeignField;

        private bool amountForeignFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal amountHome
        {
            get
            {
                return amountHomeField;
            }
            set
            {
                amountHomeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte amountForeign
        {
            get
            {
                return amountForeignField;
            }
            set
            {
                amountForeignField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool amountForeignSpecified
        {
            get
            {
                return amountForeignFieldSpecified;
            }
            set
            {
                amountForeignFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceItem
    {

        private string textField;

        private decimal quantityField;

        private string unitField;

        private decimal coefficientField;

        private bool payVATField;

        private string rateVATField;

        private decimal discountPercentageField;

        private invoiceInvoiceItemHomeCurrency homeCurrencyField;

        private invoiceInvoiceItemAccounting accountingField;

        private invoiceInvoiceItemForeignCurrency foreignCurrencyField;

        private invoiceInvoiceItemCentre centreField;

        private bool pDPField;

        private string codeField;

        private invoiceInvoiceItemActivity activityField;

        private invoiceInvoiceItemContract contractField;

        /// <remarks/>
        public string text
        {
            get
            {
                return textField;
            }
            set
            {
                textField = value;
            }
        }

        /// <remarks/>
        public decimal quantity
        {
            get
            {
                return quantityField;
            }
            set
            {
                quantityField = value;
            }
        }

        /// <remarks/>
        public string unit
        {
            get
            {
                return unitField;
            }
            set
            {
                unitField = value;
            }
        }

        /// <remarks/>
        public decimal coefficient
        {
            get
            {
                return coefficientField;
            }
            set
            {
                coefficientField = value;
            }
        }

        /// <remarks/>
        public bool payVAT
        {
            get
            {
                return payVATField;
            }
            set
            {
                payVATField = value;
            }
        }

        /// <remarks/>
        public string rateVAT
        {
            get
            {
                return rateVATField;
            }
            set
            {
                rateVATField = value;
            }
        }

        /// <remarks/>
        public decimal discountPercentage
        {
            get
            {
                return discountPercentageField;
            }
            set
            {
                discountPercentageField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceItemHomeCurrency homeCurrency
        {
            get
            {
                return homeCurrencyField;
            }
            set
            {
                homeCurrencyField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceItemAccounting accounting
        {
            get
            {
                return accountingField;
            }
            set
            {
                accountingField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceItemForeignCurrency foreignCurrency
        {
            get
            {
                return foreignCurrencyField;
            }
            set
            {
                foreignCurrencyField = value;
            }
        }

        /// <remarks/>
        public bool PDP
        {
            get
            {
                return pDPField;
            }
            set
            {
                pDPField = value;
            }
        }

        public string code
        {
            get
            {
                return codeField;
            }
            set
            {
                codeField = value;
            }
        }

        public invoiceInvoiceItemCentre centre
        {
            get
            {
                return centreField;
            }
            set
            {
                centreField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceItemActivity activity
        {
            get
            {
                return activityField;
            }
            set
            {
                activityField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceItemContract contract
        {
            get
            {
                return contractField;
            }
            set
            {
                contractField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceItemHomeCurrency
    {

        private decimal unitPriceField;

        private decimal priceField;

        private decimal priceVATField;

        private decimal priceSumField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal unitPrice
        {
            get
            {
                return unitPriceField;
            }
            set
            {
                unitPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal price
        {
            get
            {
                return priceField;
            }
            set
            {
                priceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceVAT
        {
            get
            {
                return priceVATField;
            }
            set
            {
                priceVATField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceSum
        {
            get
            {
                return priceSumField;
            }
            set
            {
                priceSumField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceItemAccounting
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return idsField;
            }
            set
            {
                idsField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceItemForeignCurrency
    {

        private decimal unitPriceField;

        private decimal priceField;

        private decimal priceVATField;

        private decimal priceSumField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal unitPrice
        {
            get
            {
                return unitPriceField;
            }
            set
            {
                unitPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal price
        {
            get
            {
                return priceField;
            }
            set
            {
                priceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceVAT
        {
            get
            {
                return priceVATField;
            }
            set
            {
                priceVATField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceSum
        {
            get
            {
                return priceSumField;
            }
            set
            {
                priceSumField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceItemActivity
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return idsField;
            }
            set
            {
                idsField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceItemContract
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return idsField;
            }
            set
            {
                idsField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceSummary
    {

        private string roundingDocumentField;

        private string roundingVATField;

        private invoiceInvoiceSummaryHomeCurrency homeCurrencyField;

        private invoiceInvoiceSummaryForeignCurrency foreignCurrencyField;

        /// <remarks/>
        public string roundingDocument
        {
            get
            {
                return roundingDocumentField;
            }
            set
            {
                roundingDocumentField = value;
            }
        }

        /// <remarks/>
        public string roundingVAT
        {
            get
            {
                return roundingVATField;
            }
            set
            {
                roundingVATField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceSummaryHomeCurrency homeCurrency
        {
            get
            {
                return homeCurrencyField;
            }
            set
            {
                homeCurrencyField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceSummaryForeignCurrency foreignCurrency
        {
            get
            {
                return foreignCurrencyField;
            }
            set
            {
                foreignCurrencyField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceSummaryHomeCurrency
    {

        private decimal priceNoneField;

        private byte priceLowField;

        private byte priceLowVATField;

        private byte priceLowSumField;

        private decimal priceHighField;

        private decimal priceHighVATField;

        private decimal priceHighSumField;

        private byte price3Field;

        private byte price3VATField;

        private byte price3SumField;

        private round roundField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceNone
        {
            get
            {
                return priceNoneField;
            }
            set
            {
                priceNoneField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte priceLow
        {
            get
            {
                return priceLowField;
            }
            set
            {
                priceLowField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte priceLowVAT
        {
            get
            {
                return priceLowVATField;
            }
            set
            {
                priceLowVATField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte priceLowSum
        {
            get
            {
                return priceLowSumField;
            }
            set
            {
                priceLowSumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceHigh
        {
            get
            {
                return priceHighField;
            }
            set
            {
                priceHighField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceHighVAT
        {
            get
            {
                return priceHighVATField;
            }
            set
            {
                priceHighVATField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceHighSum
        {
            get
            {
                return priceHighSumField;
            }
            set
            {
                priceHighSumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte price3
        {
            get
            {
                return price3Field;
            }
            set
            {
                price3Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte price3VAT
        {
            get
            {
                return price3VATField;
            }
            set
            {
                price3VATField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte price3Sum
        {
            get
            {
                return price3SumField;
            }
            set
            {
                price3SumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public round round
        {
            get
            {
                return roundField;
            }
            set
            {
                roundField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd", IsNullable = false)]
    public partial class round
    {

        private byte priceRoundField;

        /// <remarks/>
        public byte priceRound
        {
            get
            {
                return priceRoundField;
            }
            set
            {
                priceRoundField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceSummaryForeignCurrency
    {

        private currency currencyField;

        private decimal rateField;

        private decimal amountField;

        private decimal priceSumField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public currency currency
        {
            get
            {
                return currencyField;
            }
            set
            {
                currencyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal rate
        {
            get
            {
                return rateField;
            }
            set
            {
                rateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal amount
        {
            get
            {
                return amountField;
            }
            set
            {
                amountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceSum
        {
            get
            {
                return priceSumField;
            }
            set
            {
                priceSumField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd", IsNullable = false)]
    public partial class currency
    {

        private string idsField;

        /// <remarks/>
        public string ids
        {
            get
            {
                return idsField;
            }
            set
            {
                idsField = value;
            }
        }
    }


    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderCentre
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return idsField;
            }
            set
            {
                idsField = value;
            }
        }
    }

    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceItemCentre
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return idsField;
            }
            set
            {
                idsField = value;
            }
        }
    }

}
