﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class LibiadaWebEntities : DbContext
    {
        public LibiadaWebEntities()
            : base("name=LibiadaWebEntities")
        {
    		Database.CommandTimeout = 1000000;
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AccordanceCharacteristic> AccordanceCharacteristic { get; set; }
        public virtual DbSet<BinaryCharacteristic> BinaryCharacteristic { get; set; }
        public virtual DbSet<CommonSequence> CommonSequence { get; set; }
        public virtual DbSet<Characteristic> Characteristic { get; set; }
        public virtual DbSet<CharacteristicGroup> CharacteristicGroup { get; set; }
        public virtual DbSet<CharacteristicType> CharacteristicType { get; set; }
        public virtual DbSet<CharacteristicTypeLink> CharacteristicTypeLink { get; set; }
        public virtual DbSet<DnaSequence> DnaSequence { get; set; }
        public virtual DbSet<Subsequence> Subsequence { get; set; }
        public virtual DbSet<Position> Position { get; set; }
        public virtual DbSet<Element> Element { get; set; }
        public virtual DbSet<LiteratureSequence> LiteratureSequence { get; set; }
        public virtual DbSet<Matter> Matter { get; set; }
        public virtual DbSet<SequenceAttribute> SequenceAttribute { get; set; }
        public virtual DbSet<Feature> Feature { get; set; }
        public virtual DbSet<Translator> Translator { get; set; }
        public virtual DbSet<RemoteDb> RemoteDb { get; set; }
        public virtual DbSet<Fmotiv> Fmotiv { get; set; }
        public virtual DbSet<CongenericCharacteristic> CongenericCharacteristic { get; set; }
        public virtual DbSet<Instrument> Instrument { get; set; }
        public virtual DbSet<Measure> Measure { get; set; }
        public virtual DbSet<MusicSequence> MusicSequence { get; set; }
        public virtual DbSet<DataSequence> DataSequence { get; set; }
        public virtual DbSet<Note> Note { get; set; }
        public virtual DbSet<Pitch> Pitch { get; set; }
        public virtual DbSet<FmotivType> FmotivType { get; set; }
        public virtual DbSet<NoteSymbol> NoteSymbol { get; set; }
    }
}
