namespace LibiadaWeb
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The binary characteristic.
    /// </summary>
    [MetadataType(typeof(BinaryCharacteristicDataAnnotations))]
    public partial class BinaryCharacteristic
    {
    }

    /// <summary>
    /// The binary characteristic data annotations.
    /// </summary>
    public class BinaryCharacteristicDataAnnotations : CharacteristicDataAnnotations
    {
        /// <summary>
        /// Gets or sets the first element id.
        /// </summary>
        [Display(Name = "First element")]
        public int FirstElementId { get; set; }

        /// <summary>
        /// Gets or sets the second element id.
        /// </summary>
        [Display(Name = "Second element")]
        public int SecondElementId { get; set; }
    }

    /// <summary>
    /// The common sequence.
    /// </summary>
    [MetadataType(typeof(CommonSequenceDataAnnotations))]
    public partial class CommonSequence
    {
    }

    /// <summary>
    /// The sequence data annotations.
    /// </summary>
    public class CommonSequenceDataAnnotations
    {
        /// <summary>
        /// Gets or sets the notation id.
        /// </summary>
        [Display(Name = "Notation of elements in sequence")]
        public Notation Notation { get; set; }

        /// <summary>
        /// Gets or sets the matter id.
        /// </summary>
        [Display(Name = "Matter of sequence")]
        public long MatterId { get; set; }

        /// <summary>
        /// Gets or sets the remote id.
        /// </summary>
        [Display(Name = "Id in remote database")]
        public string RemoteId { get; set; }
    }

    /// <summary>
    /// The characteristic.
    /// </summary>
    [MetadataType(typeof(CharacteristicDataAnnotations))]
    public partial class Characteristic
    {
    }

    /// <summary>
    /// The characteristic data annotations.
    /// </summary>
    public class CharacteristicDataAnnotations
    {
        /// <summary>
        /// Gets or sets the sequence id.
        /// </summary>
        [Display(Name = "Sequence")]
        public long SequenceId { get; set; }
    }

    /// <summary>
    /// The characteristic type.
    /// </summary>
    [MetadataType(typeof(CharacteristicTypeDataAnnotations))]
    public partial class CharacteristicType
    {
    }

    /// <summary>
    /// The characteristic type data annotations.
    /// </summary>
    public class CharacteristicTypeDataAnnotations
    {
        /// <summary>
        /// Gets or sets the characteristic group id.
        /// </summary>
        [Display(Name = "Group Characteristic belogs to")]
        public int CharacteristicGroupId { get; set; }

        /// <summary>
        /// Gets or sets the class name.
        /// </summary>
        [Display(Name = "Calculator class name")]
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether applicable to full sequence.
        /// </summary>
        [Display(Name = "Applicable to full sequence")]
        public bool FullSequenceApplicable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether applicable to binary sequence.
        /// </summary>
        [Display(Name = "Applicable to binary sequence")]
        public bool BinarySequenceApplicable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether applicable to congeneric sequence.
        /// </summary>
        [Display(Name = "Applicable to congeneric sequence")]
        public bool CongenericSequenceApplicable { get; set; }
    }

    /// <summary>
    /// The dna sequence.
    /// </summary>
    [MetadataType(typeof(DnaSequenceDataAnnotations))]
    public partial class DnaSequence
    {
    }

    /// <summary>
    /// The dna sequence data annotations.
    /// </summary>
    public class DnaSequenceDataAnnotations : CommonSequenceDataAnnotations
    {
        /// <summary>
        /// Gets or sets a value indicating whether partial.
        /// </summary>
        [Display(Name = "Sequence is partial (incomplete)")]
        public bool Partial { get; set; }
    }

    /// <summary>
    /// The dna sequence.
    /// </summary>
    [MetadataType(typeof(DataSequenceDataAnnotations))]
    public partial class DataSequence
    {
    }

    /// <summary>
    /// The dna sequence data annotations.
    /// </summary>
    public class DataSequenceDataAnnotations : CommonSequenceDataAnnotations
    {
    }

    /// <summary>
    /// The fmotiv.
    /// </summary>
    [MetadataType(typeof(FmotivDataAnnotations))]
    public partial class Fmotiv
    {
    }

    /// <summary>
    /// The fmotiv data annotations.
    /// </summary>
    public class FmotivDataAnnotations : CommonSequenceDataAnnotations
    {
        /// <summary>
        /// Gets or sets the fmotiv_type_id.
        /// </summary>
        [Display(Name = "Fmotif type")]
        public int FmotivTypeId { get; set; }
    }

    /// <summary>
    /// The congeneric_characteristic.
    /// </summary>
    [MetadataType(typeof(CongenericCharacteristicDataAnnotations))]
    public partial class CongenericCharacteristic
    {
    }

    /// <summary>
    /// The congeneric characteristic data annotations.
    /// </summary>
    public class CongenericCharacteristicDataAnnotations : CharacteristicDataAnnotations
    {
        /// <summary>
        /// Gets or sets the element_id.
        /// </summary>
        [Display(Name = "Element")]
        public int ElementId { get; set; }
    }

    /// <summary>
    /// The literature sequence.
    /// </summary>
    [MetadataType(typeof(LiteratureSequenceDataAnnotations))]
    public partial class LiteratureSequence
    {
    }

    /// <summary>
    /// The literature sequence data annotations.
    /// </summary>
    public class LiteratureSequenceDataAnnotations : CommonSequenceDataAnnotations
    {
        /// <summary>
        /// Gets or sets the language id.
        /// </summary>
        [Display(Name = "Language of literary work")]
        public Language Language { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether literary work is in original language (not in translation).
        /// </summary>
        [Display(Name = "Literary work is in original language (not in translation)")]
        public bool Original { get; set; }
    }

    /// <summary>
    /// The measure.
    /// </summary>
    [MetadataType(typeof(MeasureDataAnnotations))]
    public partial class Measure
    {
    }

    /// <summary>
    /// The measure data annotations.
    /// </summary>
    public class MeasureDataAnnotations : CommonSequenceDataAnnotations
    {
        /// <summary>
        /// Gets or sets the beats.
        /// </summary>
        [Display(Name = "? я чото п")]
        public int Beats { get; set; }

        /// <summary>
        /// Gets or sets the beatbase.
        /// </summary>
        [Display(Name = "? я чото п")]
        public int Beatbase { get; set; }

        /// <summary>
        /// Gets or sets the ticks_per_beat.
        /// </summary>
        [Display(Name = "? я чото п")]
        public int TicksPerBeat { get; set; }

        /// <summary>
        /// Gets or sets the fifths.
        /// </summary>
        [Display(Name = "? я чото п")]
        public int Fifths { get; set; }
    }

    /// <summary>
    /// The music sequence.
    /// </summary>
    [MetadataType(typeof(MusicSequenceDataAnnotations))]
    public partial class MusicSequence
    {
    }

    /// <summary>
    /// The music sequence data annotations.
    /// </summary>
    public class MusicSequenceDataAnnotations : CommonSequenceDataAnnotations
    {
    }

    /// <summary>
    /// The note.
    /// </summary>
    [MetadataType(typeof(NoteDataAnnotations))]
    public partial class Note
    {
    }

    /// <summary>
    /// The note data annotations.
    /// </summary>
    public class NoteDataAnnotations
    {
        /// <summary>
        /// Gets or sets the numerator.
        /// </summary>
        [Display(Name = "Числитель в дроби доли")]
        public int Numerator { get; set; }

        /// <summary>
        /// Gets or sets the denominator.
        /// </summary>
        [Display(Name = "Знаменатель в дроби доли")]
        public int Denominator { get; set; }

        /// <summary>
        /// Gets or sets the onumerator.
        /// </summary>
        [Display(Name = "Оригинальный числитель в дроби доли")]
        public int Onumerator { get; set; }

        /// <summary>
        /// Gets or sets the odenominator.
        /// </summary>
        [Display(Name = "Оригинальный знаменатель в дроби доли")]
        public int Odenominator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether triplet.
        /// </summary>
        [Display(Name = "Триоль")]
        public bool Triplet { get; set; }

        /// <summary>
        /// Gets or sets the tie id.
        /// </summary>
        [Display(Name = "Лига")]
        public int TieId { get; set; }
    }

    /// <summary>
    /// The pitch.
    /// </summary>
    [MetadataType(typeof(PitchDataAnnotations))]
    public partial class Pitch
    {
    }

    /// <summary>
    /// The pitch data annotations.
    /// </summary>
    public class PitchDataAnnotations
    {
        /// <summary>
        /// Gets or sets the octave.
        /// </summary>
        [Display(Name = "Номер октавы")]
        public int Octave { get; set; }

        /// <summary>
        /// Gets or sets the midinumber.
        /// </summary>
        [Display(Name = "Уникальный номер ноты по миди стандарту")]
        public int Midinumber { get; set; }

        /// <summary>
        /// Gets or sets the instrument id.
        /// </summary>
        [Display(Name = "Instrument")]
        public int InstrumentId { get; set; }

        /// <summary>
        /// Gets or sets the accidental id.
        /// </summary>
        [Display(Name = "Знак альтерации")]
        public int AccidentalId { get; set; }

        /// <summary>
        /// Gets or sets the note symbol id.
        /// </summary>
        [Display(Name = "Note Symbol")]
        public int NoteSymbol { get; set; }
    }
}
