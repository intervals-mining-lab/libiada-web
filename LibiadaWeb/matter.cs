//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LibiadaWeb
{
    using System;
    using System.Collections.Generic;
    
    public partial class Matter
    {
        public Matter()
        {
            Sequence = new HashSet<CommonSequence>();
            DnaSequence = new HashSet<DnaSequence>();
            LiteratureSequence = new HashSet<LiteratureSequence>();
            MusicSequence = new HashSet<MusicSequence>();
            DataSequence = new HashSet<DataSequence>();
            SequenceGroup = new HashSet<SequenceGroup>();
            ImageSequence = new HashSet<ImageSequence>();
        }
    
        public long Id { get; set; }
        public string Name { get; set; }
        public Nature Nature { get; set; }
        public SequenceType SequenceType { get; set; }
        public Group Group { get; set; }
        public string Description { get; set; }
        public System.DateTimeOffset Created { get; set; }
        public System.DateTimeOffset Modified { get; set; }
        public Nullable<int> MultisequenceId { get; set; }
        public Nullable<short> MultisequenceNumber { get; set; }
        public byte[] Source { get; set; }
        public string CollectionCountry { get; set; }
        public Nullable<System.DateTime> CollectionDate { get; set; }
        public string CollectionLocation { get; set; }
    
        public virtual ICollection<CommonSequence> Sequence { get; set; }
        public virtual ICollection<DnaSequence> DnaSequence { get; set; }
        public virtual ICollection<LiteratureSequence> LiteratureSequence { get; set; }
        public virtual ICollection<MusicSequence> MusicSequence { get; set; }
        public virtual ICollection<DataSequence> DataSequence { get; set; }
        public virtual ICollection<SequenceGroup> SequenceGroup { get; set; }
        public virtual Multisequence Multisequence { get; set; }
        public virtual ICollection<ImageSequence> ImageSequence { get; set; }
    }
}
