namespace LibiadaWeb
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The binary characteristic.
    /// </summary>
    [MetadataType(typeof(BinaryCharacteristicDataAnnotations))]
    public partial class BinaryCharacteristicValue
    {
    }

    /// <summary>
    /// The binary characteristic data annotations.
    /// </summary>
    public class BinaryCharacteristicDataAnnotations
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
    /// The sequence group.
    /// </summary>
    [MetadataType(typeof(SequenceGroupDataAnnotations))]
    public partial class SequenceGroup
    {
    }

    /// <summary>
    /// The sequence group data annotations.
    /// </summary>
    public class SequenceGroupDataAnnotations
    {
        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        [Display(Name = "Creator")]
        public AspNetUser Creator { get; set; }

        /// <summary>
        /// Gets or sets the modifier.
        /// </summary>
        [Display(Name = "Modifier")]
        public AspNetUser Modifier { get; set; }
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

        /// <summary>
        /// Gets or sets the remote db id.
        /// </summary>
        [Display(Name = "Remote database")]
        public string RemoteDb { get; set; }
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
    /// The fmotif.
    /// </summary>
    [MetadataType(typeof(FmotifDataAnnotations))]
    public partial class Fmotif
    {
    }

    /// <summary>
    /// The fmotif data annotations.
    /// </summary>
    public class FmotifDataAnnotations : CommonSequenceDataAnnotations
    {
        /// <summary>
        /// Gets or sets the fmotif_type_id.
        /// </summary>
        [Display(Name = "Fmotif type")]
        public int FmotifType { get; set; }
    }

    /// <summary>
    /// The congeneric_characteristic.
    /// </summary>
    [MetadataType(typeof(CongenericCharacteristicDataAnnotations))]
    public partial class CongenericCharacteristicValue
    {
    }

    /// <summary>
    /// The congeneric characteristic data annotations.
    /// </summary>
    public class CongenericCharacteristicDataAnnotations
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
        /// <summary>
        /// Gets or sets the matter id.
        /// </summary>
        [Display(Name = "Musical sequences")]
        public long MatterId { get; set; }

        /// <summary>
        /// Gets or sets the notation id.
        /// </summary>
        [Display(Name = "Notation")]
        public Notation Notation { get; set; }

        /// <summary>
        /// Gets or sets the pause treatment.
        /// </summary>
        [Display(Name = "Pause treatment")]
        public short PauseTreatment { get; set; }

        /// <summary>
        /// Gets or sets the sequential transfer.
        /// </summary>
        [Display(Name = "Sequential transfer")]
        public bool SequentialTransfer { get; set; }
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
        /// Gets or sets the note symbol id.
        /// </summary>
        [Display(Name = "Note Symbol")]
        public int NoteSymbol { get; set; }
    }
}
