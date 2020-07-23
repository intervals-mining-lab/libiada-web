using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LibiadaWeb.Models.NcbiSequencesData
{
    public class NcbiXmlDataAnnotations
    {
    }

    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class eSummaryResult
    {

        [XmlElement("DocSum")]
        public ESummaryResultDocumentSummary[] DocumentSummary { get; set; }
    }

    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class ESummaryResultDocumentSummary
    {
        public uint Id { get; set; }

        [XmlElement("Item")]
        public eSummaryResultDocSumItem[] Items { get; set; }
    }

    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class eSummaryResultDocSumItem
    {

        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public string Type { get; set; }


        [XmlText()]
        public string Value { get; set; }
    }
}
