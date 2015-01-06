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
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [Display(Name = "Описание")]
        public string description { get; set; }
    }

    /// <summary>
    /// The accidental.
    /// </summary>
    [MetadataType(typeof(AccidentalDataAnnotations))]
    public partial class accidental
    {
    }

    /// <summary>
    /// The accidental data annotations.
    /// </summary>
    public class AccidentalDataAnnotations : CommonDataAnnotations
    {
    }

    /// <summary>
    /// The alphabet.
    /// </summary>
    [MetadataType(typeof(AlphabetDataAnnotations))]
    public partial class alphabet
    {
    }

    /// <summary>
    /// The alphabet data annotations.
    /// </summary>
    public class AlphabetDataAnnotations
    {
        /// <summary>
        /// Gets or sets the chain_id.
        /// </summary>
        [Display(Name = "Принадлежит цепочке")]
        public long chain_id { get; set; }

        /// <summary>
        /// Gets or sets the element_id.
        /// </summary>
        [Display(Name = "Содержит элемент")]
        public int element_id { get; set; }

        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        [Display(Name = "Номер элемента в алфавите")]
        public int number { get; set; }
    }

    /// <summary>
    /// The binary_characteristic.
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
        /// Gets or sets the first_element_id.
        /// </summary>
        [Display(Name = "Первый элемент")]
        public int first_element_id { get; set; }

        /// <summary>
        /// Gets or sets the second_element_id.
        /// </summary>
        [Display(Name = "Второй элемент")]
        public int second_element_id { get; set; }
    }

    /// <summary>
    /// The chain.
    /// </summary>
    [MetadataType(typeof(ChainDataAnnotations))]
    public partial class CommonSequence
    {
    }

    /// <summary>
    /// The chain data annotations.
    /// </summary>
    public class ChainDataAnnotations
    {
        /// <summary>
        /// Gets or sets the notation_id.
        /// </summary>
        [Display(Name = "Форма записи")]
        public long notation_id { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        [Display(Name = "Дата создания")]
        public DateTimeOffset created { get; set; }

        /// <summary>
        /// Gets or sets the matter_id.
        /// </summary>
        [Display(Name = "Принадлежит объекту исследования")]
        public long matter_id { get; set; }

        /// <summary>
        /// Gets or sets the piece_type_id.
        /// </summary>
        [Display(Name = "Тип фрагмента цепочки")]
        public int piece_type_id { get; set; }

        /// <summary>
        /// Gets or sets the piece_position.
        /// </summary>
        [Display(Name = "Позиция с которой начинается цепочка")]
        public long piece_position { get; set; }

        /// <summary>
        /// Gets or sets the remote_db_id.
        /// </summary>
        [Display(Name = "Сторонняя БД")]
        public int remote_db_id { get; set; }

        /// <summary>
        /// Gets or sets the remote_id.
        /// </summary>
        [Display(Name = "id в сторонней БД")]
        public string remote_id { get; set; }
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
        /// Gets or sets the chain_id.
        /// </summary>
        [Display(Name = "Принадлежит цепочке")]
        public long chain_id { get; set; }

        /// <summary>
        /// Gets or sets the characteristic_type_id.
        /// </summary>
        [Display(Name = "Тип характеристики")]
        public int characteristic_type_id { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [Display(Name = "Значение")]
        public double value { get; set; }

        /// <summary>
        /// Gets or sets the value_string.
        /// </summary>
        [Display(Name = "Значение в виде строки")]
        public string value_string { get; set; }

        /// <summary>
        /// Gets or sets the link_id.
        /// </summary>
        [Display(Name = "Привязка")]
        public int link_id { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        [Display(Name = "Дата создания")]
        public DateTimeOffset created { get; set; }
    }

    /// <summary>
    /// The characteristic_group.
    /// </summary>
    [MetadataType(typeof(CharacteristicGroupDataAnnotations))]
    public partial class characteristic_group
    {
    }

    /// <summary>
    /// The characteristic group data annotations.
    /// </summary>
    public class CharacteristicGroupDataAnnotations : CommonDataAnnotations
    {
    }

    /// <summary>
    /// The characteristic_type.
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
        /// Gets or sets the characteristic_group_id.
        /// </summary>
        [Display(Name = "Группа характеристик")]
        public int characteristic_group_id { get; set; }

        /// <summary>
        /// Gets or sets the class_name.
        /// </summary>
        [Display(Name = "Класс-калькулятор")]
        public string class_name { get; set; }

        /// <summary>
        /// Gets or sets the linkable.
        /// </summary>
        [Display(Name = "Привязываемость")]
        public int linkable { get; set; }

        /// <summary>
        /// Gets or sets the full_chain_applicable.
        /// </summary>
        [Display(Name = "Применимо к полным цепочкам")]
        public int full_chain_applicable { get; set; }

        /// <summary>
        /// Gets or sets the binary_chain_applicable.
        /// </summary>
        [Display(Name = "Применимо к бинарным цепочкам")]
        public int binary_chain_applicable { get; set; }

        /// <summary>
        /// Gets or sets the congeneric_chain_applicable.
        /// </summary>
        [Display(Name = "Применимо к однородным цепочкам")]
        public int congeneric_chain_applicable { get; set; }
    }

    /// <summary>
    /// The dna_chain.
    /// </summary>
    [MetadataType(typeof(DnaChainDataAnnotations))]
    public partial class DnaSequence
    {
    }

    /// <summary>
    /// The dna chain data annotations.
    /// </summary>
    public class DnaChainDataAnnotations : ChainDataAnnotations
    {
        /// <summary>
        /// Gets or sets the fasta_header.
        /// </summary>
        [Display(Name = "Заголовок FASTA файла")]
        public string fasta_header { get; set; }
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
        public string value { get; set; }

        /// <summary>
        /// Gets or sets the notation_id.
        /// </summary>
        [Display(Name = "Форма записи")]
        public long notation_id { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        [Display(Name = "Дата создания")]
        public DateTimeOffset created { get; set; }
    }

    /// <summary>
    /// The fmotiv.
    /// </summary>
    [MetadataType(typeof(FmotivDataAnnotations))]
    public partial class fmotiv
    {
    }

    /// <summary>
    /// The fmotiv data annotations.
    /// </summary>
    public class FmotivDataAnnotations : ChainDataAnnotations
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [Display(Name = "Значение")]
        public string value { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Display(Name = "Название")]
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [Display(Name = "Описание")]
        public string description { get; set; }

        /// <summary>
        /// Gets or sets the fmotiv_type_id.
        /// </summary>
        [Display(Name = "Тип ф-мотива")]
        public int fmotiv_type_id { get; set; }
    }

    /// <summary>
    /// The fmotiv_type.
    /// </summary>
    [MetadataType(typeof(FmotivTypeDataAnnotations))]
    public partial class fmotiv_type
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
    public partial class congeneric_characteristic
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
        public int element_id { get; set; }
    }

    /// <summary>
    /// The instrument.
    /// </summary>
    [MetadataType(typeof(InstrumentDataAnnotations))]
    public partial class instrument
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
    public partial class language
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
    public partial class link
    {
    }

    /// <summary>
    /// The link data annotations.
    /// </summary>
    public class LinkDataAnnotations : CommonDataAnnotations
    {
    }

    /// <summary>
    /// The literature_chain.
    /// </summary>
    [MetadataType(typeof(LiteratureChainDataAnnotations))]
    public partial class literature_chain
    {
    }

    /// <summary>
    /// The literature chain data annotations.
    /// </summary>
    public class LiteratureChainDataAnnotations : ChainDataAnnotations
    {
        /// <summary>
        /// Gets or sets the language_id.
        /// </summary>
        [Display(Name = "Язык")]
        public int language_id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether original.
        /// </summary>
        [Display(Name = "Оригинал")]
        public bool original { get; set; }
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
        /// Gets or sets the nature_id.
        /// </summary>
        [Display(Name = "Природа")]
        public int nature_id { get; set; }
    }

    /// <summary>
    /// The measure.
    /// </summary>
    [MetadataType(typeof(MeasureDataAnnotations))]
    public partial class measure
    {
    }

    /// <summary>
    /// The measure data annotations.
    /// </summary>
    public class MeasureDataAnnotations : ChainDataAnnotations
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [Display(Name = "Значение")]
        public string value { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Display(Name = "Название")]
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [Display(Name = "Описание")]
        public string description { get; set; }

        /// <summary>
        /// Gets or sets the beats.
        /// </summary>
        [Display(Name = "? я чото п")]
        public int beats { get; set; }

        /// <summary>
        /// Gets or sets the beatbase.
        /// </summary>
        [Display(Name = "? я чото п")]
        public int beatbase { get; set; }

        /// <summary>
        /// Gets or sets the ticks_per_beat.
        /// </summary>
        [Display(Name = "? я чото п")]
        public int ticks_per_beat { get; set; }

        /// <summary>
        /// Gets or sets the fifths.
        /// </summary>
        [Display(Name = "? я чото п")]
        public int fifths { get; set; }
    }

    /// <summary>
    /// The music_chain.
    /// </summary>
    [MetadataType(typeof(MusicChainDataAnnotations))]
    public partial class MusicSequence
    {
    }

    /// <summary>
    /// The music chain data annotations.
    /// </summary>
    public class MusicChainDataAnnotations : ChainDataAnnotations
    {
    }

    /// <summary>
    /// The nature.
    /// </summary>
    [MetadataType(typeof(NatureDataAnnotations))]
    public partial class nature
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
    public partial class notation
    {
    }

    /// <summary>
    /// The notation data annotations.
    /// </summary>
    public class NotationDataAnnotations : CommonDataAnnotations
    {
        /// <summary>
        /// Gets or sets the nature_id.
        /// </summary>
        [Display(Name = "Природа")]
        public int nature_id { get; set; }
    }

    /// <summary>
    /// The note.
    /// </summary>
    [MetadataType(typeof(NoteDataAnnotations))]
    public partial class note
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
        public int numerator { get; set; }

        /// <summary>
        /// Gets or sets the denominator.
        /// </summary>
        [Display(Name = "Знаменатель в дроби доли")]
        public int denominator { get; set; }

        /// <summary>
        /// Gets or sets the ticks.
        /// </summary>
        [Display(Name = "Количество МИДИ тиков в доле")]
        public int ticks { get; set; }

        /// <summary>
        /// Gets or sets the onumerator.
        /// </summary>
        [Display(Name = "Оригинальный числитель в дроби доли")]
        public int onumerator { get; set; }

        /// <summary>
        /// Gets or sets the odenominator.
        /// </summary>
        [Display(Name = "Оригинальный знаменатель в дроби доли")]
        public int odenominator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether triplet.
        /// </summary>
        [Display(Name = "Триоль")]
        public bool triplet { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        [Display(Name = "Приоритет")]
        public int priority { get; set; }

        /// <summary>
        /// Gets or sets the tie_id.
        /// </summary>
        [Display(Name = "Лига")]
        public int tie_id { get; set; }
    }

    /// <summary>
    /// The note_symbol.
    /// </summary>
    [MetadataType(typeof(NoteSymbolDataAnnotations))]
    public partial class note_symbol
    {
    }

    /// <summary>
    /// The note symbol data annotations.
    /// </summary>
    public class NoteSymbolDataAnnotations : CommonDataAnnotations
    {
    }

    /// <summary>
    /// The piece_type.
    /// </summary>
    [MetadataType(typeof(PieceTypeDataAnnotations))]
    public partial class piece_type
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
    public partial class pitch
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
        public int octave { get; set; }

        /// <summary>
        /// Gets or sets the midinumber.
        /// </summary>
        [Display(Name = "Уникальный номер ноты по миди стандарту")]
        public int midinumber { get; set; }

        /// <summary>
        /// Gets or sets the instrument_id.
        /// </summary>
        [Display(Name = "Инструмент")]
        public int instrument_id { get; set; }

        /// <summary>
        /// Gets or sets the note_id.
        /// </summary>
        [Display(Name = "Принадлежит ноте")]
        public long note_id { get; set; }

        /// <summary>
        /// Gets or sets the accidental_id.
        /// </summary>
        [Display(Name = "Знак альтерации")]
        public int accidental_id { get; set; }

        /// <summary>
        /// Gets or sets the note_symbol_id.
        /// </summary>
        [Display(Name = "Обозначение ноты")]
        public int note_symbol_id { get; set; }
    }

    /// <summary>
    /// The remote_db.
    /// </summary>
    [MetadataType(typeof(RemoteDbDataAnnotations))]
    public partial class remote_db
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
        public string url { get; set; }
    }

    /// <summary>
    /// The tie.
    /// </summary>
    [MetadataType(typeof(TieDataAnnotations))]
    public partial class tie
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
    public partial class product
    {
    }

    /// <summary>
    /// The product data annotations.
    /// </summary>
    public class ProductDataAnnotations : CommonDataAnnotations
    {
        /// <summary>
        /// Gets or sets the piece_type_id.
        /// </summary>
        [Display(Name = "Тип фрагмента цепочки")]
        public int piece_type_id { get; set; }
    }

    /// <summary>
    /// The translator.
    /// </summary>
    [MetadataType(typeof(TranslatorDataAnnotations))]
    public partial class translator
    {
    }

    /// <summary>
    /// The translator data annotations.
    /// </summary>
    public class TranslatorDataAnnotations : CommonDataAnnotations
    {
    }
}
