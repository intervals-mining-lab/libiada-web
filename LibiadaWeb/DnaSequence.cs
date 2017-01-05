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
    
    public partial class DnaSequence
    {
        public DnaSequence()
        {
            BinaryCharacteristic = new HashSet<BinaryCharacteristic>();
            Characteristic = new HashSet<Characteristic>();
            Subsequence = new HashSet<Subsequence>();
            SequenceAttribute = new HashSet<SequenceAttribute>();
        }
    
        public long Id { get; set; }
        public Notation Notation { get; set; }
        public System.DateTimeOffset Created { get; set; }
        public long MatterId { get; set; }
        public Nullable<int> RemoteDbId { get; set; }
        public string RemoteId { get; set; }
        public System.DateTimeOffset Modified { get; set; }
        public bool Partial { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<BinaryCharacteristic> BinaryCharacteristic { get; set; }
        public virtual ICollection<Characteristic> Characteristic { get; set; }
        public virtual Matter Matter { get; set; }
        public virtual RemoteDb RemoteDb { get; set; }
        public virtual ICollection<Subsequence> Subsequence { get; set; }
        public virtual ICollection<SequenceAttribute> SequenceAttribute { get; set; }
    }
}
