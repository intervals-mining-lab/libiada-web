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
    
    public partial class CongenericCharacteristic
    {
        public long Id { get; set; }
        public long SequenceId { get; set; }
        public int CharacteristicTypeId { get; set; }
        public double Value { get; set; }
        public string ValueString { get; set; }
        public Nullable<int> LinkId { get; set; }
        public System.DateTimeOffset Created { get; set; }
        public long ElementId { get; set; }
        public Nullable<System.DateTimeOffset> Modified { get; set; }
    
        public virtual CharacteristicType CharacteristicType { get; set; }
        public virtual Element Element { get; set; }
        public virtual Link Link { get; set; }
    }
}