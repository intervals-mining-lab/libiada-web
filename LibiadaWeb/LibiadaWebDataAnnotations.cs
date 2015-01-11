namespace LibiadaWeb
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The common data annotations.
    /// </summary>
    public class CommonDataAnnotations
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Display(Name = "Название")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [Display(Name = "Описание")]
        public string Description { get; set; }
    }

    /// <summary>
    /// The accidental.
    /// </summary>
    [MetadataType(typeof(AccidentalDataAnnotations))]
    public partial class Accidental 
    {
    }

    /// <summary>
    /// The accidental data annotations.
    /// </summary>
    public class AccidentalDataAnnotations : CommonDataAnnotations
    {
    }

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
        public long NotationId { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        [Display(Name = "Дата создания")]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// Gets or sets the modified.
        /// </summary>
        [Display(Name = "Last modification date")]
        public DateTimeOffset Modified { get; set; }

        /// <summary>
        /// Gets or sets the matter id.
        /// </summary>
        [Display(Name = "Принадлежит объекту исследования")]
        public long MatterId { get; set; }

        /// <summary>
        /// Gets or sets the piece type id.
        /// </summary>
        [Display(Name = "Тип фрагмента цепочки")]
        public int PieceTypeId { get; set; }

        /// <summary>
        /// Gets or sets the piece position.
        /// </summary>
        [Display(Name = "Позиция с которой начинается цепочка")]
        public long PiecePosition { get; set; }

        /// <summary>
        /// Gets or sets the remote db id.
        /// </summary>
        [Display(Name = "Сторонняя БД")]
        public int RemoteDbId { get; set; }

        /// <summary>
        /// Gets or sets the remote id.
        /// </summary>
        [Display(Name = "id в сторонней БД")]
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
        [Display(Name = "Принадлежит цепочке")]
        public long SequenceId { get; set; }

        /// <summary>
        /// Gets or sets the characteristic type id.
        /// </summary>
        [Display(Name = "Characteristic name")]
        public int CharacteristicTypeId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [Display(Name = "Значение")]
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the value string.
        /// </summary>
        [Display(Name = "Value as string")]
        public string ValueString { get; set; }

        /// <summary>
        /// Gets or sets the link id.
        /// </summary>
        [Display(Name = "Link of characteristic")]
        public int LinkId { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        [Display(Name = "Дата создания")]
        public DateTimeOffset Created { get; set; }
    }

    /// <summary>
    /// The characteristic group.
    /// </summary>
    [MetadataType(typeof(CharacteristicGroupDataAnnotations))]
    public partial class CharacteristicGroup
    {
    }

    /// <summary>
    /// The characteristic group data annotations.
    /// </summary>
    public class CharacteristicGroupDataAnnotations : CommonDataAnnotations
    {
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
    public class CharacteristicTypeDataAnnotations : CommonDataAnnotations
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
        /// Gets or sets the linkable.
        /// </summary>
        [Display(Name = "Characteristic is linkable")]
        public int Linkable { get; set; }

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
        /// Gets or sets the fasta_header.
        /// </summary>
        [Display(Name = "FASTA file header")]
        public string FastaHeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether partial.
        /// </summary>
        [Display(Name = "Sequence is partial (incomplete)")]
        public bool Partial { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sequence complementary.
        /// </summary>
        [Display(Name = "Sequence is complementary to source sequence")]
        public bool Complementary { get; set; }

        /// <summary>
        /// Gets or sets the product id.
        /// </summary>
        [Display(Name = "Sequence product")]
        public int ProductId { get; set; }
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
    /// The gene.
    /// </summary>
    [MetadataType(typeof(GeneDataAnnotations))]
    public partial class Gene
    {
    }

    /// <summary>
    /// The gene data annotations.
    /// </summary>
    public class GeneDataAnnotations
    {
        /// <summary>
        /// Gets or sets a value indicating whether sequence complementary.
        /// </summary>
        [Display(Name = "Sequence is complementary to source sequence")]
        public bool Complementary { get; set; }

        /// <summary>
        /// Gets or sets the product id.
        /// </summary>
        [Display(Name = "Sequence product")]
        public int ProductId { get; set; }
    }

    /// <summary>
    /// The element.
    /// </summary>
    [MetadataType(typeof(ElementDataAnnotations))]
    public partial class Element
    {
    }

    /// <summary>
    /// The element data annotations.
    /// </summary>
    public class ElementDataAnnotations : CommonDataAnnotations
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [Display(Name = "Значение")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the notation_id.
        /// </summary>
        [Display(Name = "Форма записи")]
        public long NotationId { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        [Display(Name = "Дата создания")]
        public DateTimeOffset Created { get; set; }
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
        /// Gets or sets the value.
        /// </summary>
        [Display(Name = "Значение")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Display(Name = "Название")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [Display(Name = "Описание")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the fmotiv_type_id.
        /// </summary>
        [Display(Name = "Тип ф-мотива")]
        public int FmotivTypeId { get; set; }
    }

    /// <summary>
    /// The fmotiv type.
    /// </summary>
    [MetadataType(typeof(FmotivTypeDataAnnotations))]
    public partial class FmotivType
    {
    }

    /// <summary>
    /// The fmotiv type data annotations.
    /// </summary>
    public class FmotivTypeDataAnnotations : CommonDataAnnotations
    {
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
        [Display(Name = "Элемент")]
        public int ElementId { get; set; }
    }

    /// <summary>
    /// The instrument.
    /// </summary>
    [MetadataType(typeof(InstrumentDataAnnotations))]
    public partial class Instrument
    {
    }

    /// <summary>
    /// The instrument data annotations.
    /// </summary>
    public class InstrumentDataAnnotations : CommonDataAnnotations
    {
    }

    /// <summary>
    /// The language.
    /// </summary>
    [MetadataType(typeof(LanguageDataAnnotations))]
    public partial class Language
    {
    }

    /// <summary>
    /// The language data annotations.
    /// </summary>
    public class LanguageDataAnnotations : CommonDataAnnotations
    {
    }

    /// <summary>
    /// The link.
    /// </summary>
    [MetadataType(typeof(LinkDataAnnotations))]
    public partial class Link
    {
    }

    /// <summary>
    /// The link data annotations.
    /// </summary>
    public class LinkDataAnnotations : CommonDataAnnotations
    {
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
        public int LanguageId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether literary work is in original language (not in translation).
        /// </summary>
        [Display(Name = "Literary work is in original language (not in translation)")]
        public bool Original { get; set; }

        /// <summary>
        /// Gets or sets the translator id.
        /// </summary>
        [Display(Name = "Translator")]
        public int TranslatorId { get; set; }
    }

    /// <summary>
    /// The matter.
    /// </summary>
    [MetadataType(typeof(MatterDataAnnotations))]
    public partial class Matter
    {
    }

    /// <summary>
    /// The matter data annotations.
    /// </summary>
    public class MatterDataAnnotations : CommonDataAnnotations
    {
        /// <summary>
        /// Gets or sets the nature id.
        /// </summary>
        [Display(Name = "Nature")]
        public int NatureId { get; set; }
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
        /// Gets or sets the value.
        /// </summary>
        [Display(Name = "Значение")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Display(Name = "Название")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [Display(Name = "Описание")]
        public string Description { get; set; }

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
    /// The nature.
    /// </summary>
    [MetadataType(typeof(NatureDataAnnotations))]
    public partial class Nature
    {
    }

    /// <summary>
    /// The nature data annotations.
    /// </summary>
    public class NatureDataAnnotations : CommonDataAnnotations
    {
    }

    /// <summary>
    /// The notation.
    /// </summary>
    [MetadataType(typeof(NotationDataAnnotations))]
    public partial class Notation
    {
    }

    /// <summary>
    /// The notation data annotations.
    /// </summary>
    public class NotationDataAnnotations : CommonDataAnnotations
    {
        /// <summary>
        /// Gets or sets the nature id.
        /// </summary>
        [Display(Name = "Nature of notation")]
        public int NatureId { get; set; }
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
    public class NoteDataAnnotations : ElementDataAnnotations
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
        /// Gets or sets the ticks.
        /// </summary>
        [Display(Name = "Количество МИДИ тиков в доле")]
        public int Ticks { get; set; }

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
        /// Gets or sets the priority.
        /// </summary>
        [Display(Name = "Приоритет")]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the tie id.
        /// </summary>
        [Display(Name = "Лига")]
        public int TieId { get; set; }
    }

    /// <summary>
    /// The note_symbol.
    /// </summary>
    [MetadataType(typeof(NoteSymbolDataAnnotations))]
    public partial class NoteSymbol
    {
    }

    /// <summary>
    /// The note symbol data annotations.
    /// </summary>
    public class NoteSymbolDataAnnotations : CommonDataAnnotations
    {
    }

    /// <summary>
    /// The piece type.
    /// </summary>
    [MetadataType(typeof(PieceTypeDataAnnotations))]
    public partial class PieceType
    {
    }

    /// <summary>
    /// The piece type data annotations.
    /// </summary>
    public class PieceTypeDataAnnotations : NotationDataAnnotations
    {
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
        [Display(Name = "Инструмент")]
        public int InstrumentId { get; set; }

        /// <summary>
        /// Gets or sets the note_id.
        /// </summary>
        [Display(Name = "Принадлежит ноте")]
        public long NoteId { get; set; }

        /// <summary>
        /// Gets or sets the accidental id.
        /// </summary>
        [Display(Name = "Знак альтерации")]
        public int AccidentalId { get; set; }

        /// <summary>
        /// Gets or sets the note symbol id.
        /// </summary>
        [Display(Name = "Обозначение ноты")]
        public int NoteSymbolId { get; set; }
    }

    /// <summary>
    /// The remote db.
    /// </summary>
    [MetadataType(typeof(RemoteDbDataAnnotations))]
    public partial class RemoteDb
    {
    }

    /// <summary>
    /// The remote db data annotations.
    /// </summary>
    public class RemoteDbDataAnnotations : CommonDataAnnotations
    {
        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        [Display(Name = "url адрес")]
        public string Url { get; set; }
    }

    /// <summary>
    /// The tie.
    /// </summary>
    [MetadataType(typeof(TieDataAnnotations))]
    public partial class Tie
    {
    }

    /// <summary>
    /// The tie data annotations.
    /// </summary>
    public class TieDataAnnotations : CommonDataAnnotations
    {
    }

    /// <summary>
    /// The product.
    /// </summary>
    [MetadataType(typeof(ProductDataAnnotations))]
    public partial class Product
    {
    }

    /// <summary>
    /// The product data annotations.
    /// </summary>
    public class ProductDataAnnotations : CommonDataAnnotations
    {
        /// <summary>
        /// Gets or sets the piece type id.
        /// </summary>
        [Display(Name = "Type of product")]
        public int PieceTypeId { get; set; }
    }

    /// <summary>
    /// The translator.
    /// </summary>
    [MetadataType(typeof(TranslatorDataAnnotations))]
    public partial class Translator
    {
    }

    /// <summary>
    /// The translator data annotations.
    /// </summary>
    public class TranslatorDataAnnotations : CommonDataAnnotations
    {
    }
}
