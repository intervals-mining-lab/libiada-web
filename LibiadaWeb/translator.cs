namespace LibiadaWeb
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Automatic translators used for literature sequences.
    /// </summary>
    public enum Translator : byte
    {
        /// <summary>
        /// Google translate translator.
        /// </summary>
        [Display(Name = "Google Translate")]
        [Description("http://translate.google.ru/")]
        GoogleTranslate = 1,
        /// <summary>
        /// PROMT translator.
        /// </summary>
        [Display(Name = "PROMT (translate.ru)")]
        [Description("http://www.translate.ru/")]
        Promt = 2,
        /// <summary>
        /// InterTran translator.
        /// </summary>
        [Display(Name = "InterTran")]
        [Description("http://mrtranslate.ru/translators/intertran.html")]
        InterTran = 3
    }
}