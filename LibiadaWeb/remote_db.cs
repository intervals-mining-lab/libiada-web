//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LibiadaWeb
{
    using System;
    using System.Collections.Generic;
    
    public partial class remote_db
    {
        public remote_db()
        {
            this.chain = new HashSet<chain>();
            this.dna_chain = new HashSet<dna_chain>();
            this.literature_chain = new HashSet<literature_chain>();
            this.music_chain = new HashSet<music_chain>();
            this.fmotiv = new HashSet<fmotiv>();
            this.measure = new HashSet<measure>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public int nature_id { get; set; }
    
        public virtual ICollection<chain> chain { get; set; }
        public virtual ICollection<dna_chain> dna_chain { get; set; }
        public virtual ICollection<literature_chain> literature_chain { get; set; }
        public virtual ICollection<music_chain> music_chain { get; set; }
        public virtual ICollection<fmotiv> fmotiv { get; set; }
        public virtual ICollection<measure> measure { get; set; }
        public virtual nature nature { get; set; }
    }
}
