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
        /// No translator or translated manually.
        /// </summary>
        [Display(Name = "None or manual")]
        [Description("No translator is applied (text is original) or text translated manualy")]
        NoneOrManual = 0,

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
