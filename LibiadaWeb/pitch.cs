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
    
    public partial class Pitch
    {
        public Pitch()
        {
            Note = new HashSet<Note>();
        }
    
        public int Id { get; set; }
        public int Octave { get; set; }
        public int Midinumber { get; set; }
        public LibiadaMusic.ScoreModel.Instrument Instrument { get; set; }
        public LibiadaMusic.ScoreModel.Accidental Accidental { get; set; }
        public LibiadaMusic.ScoreModel.NoteSymbol NoteSymbol { get; set; }
    
        public virtual ICollection<Note> Note { get; set; }
    }
}
