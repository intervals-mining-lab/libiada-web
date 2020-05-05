namespace LibiadaWeb.Models.SequencesData
{
    class NcbiXmlDataAnnotations
    {
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class eSummaryResult
    {

        private eSummaryResultDocSum[] docSumField;

        [System.Xml.Serialization.XmlElementAttribute("DocSum")]
        public eSummaryResultDocSum[] DocSum
        {
            get
            {
                return docSumField;
            }
            set
            {
                docSumField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class eSummaryResultDocSum
    {

        private uint idField;

        private eSummaryResultDocSumItem[] itemField;

        /// <remarks/>
        public uint Id
        {
            get => idField;
            set => idField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Item")]
        public eSummaryResultDocSumItem[] Item
        {
            get => itemField;
            set => itemField = value;
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class eSummaryResultDocSumItem
    {

        private string nameField;

        private string typeField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get => nameField;
            set => nameField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get => typeField;
            set => typeField = value;
        }


        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get => valueField;
            set => valueField = value;
        }
    }
}

