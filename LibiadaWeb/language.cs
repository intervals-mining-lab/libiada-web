namespace LibiadaWeb
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public enum Language : byte
    {
        /// <summary>
        /// Russian language.
        /// </summary>
        [Display(Name = "Russian")]
        [Description("Set if literary work completely or mostly written in russian language")]
        Russian = 1,
        /// <summary>
        /// English language.
        /// </summary>
        [Display(Name = "English")]
        [Description("Set if literary work completely or mostly written in english language")]
        English = 2,
        /// <summary>
        /// German language.
        /// </summary>
        [Display(Name = "German")]
        [Description("Set if literary work completely or mostly written in german language")]
        German = 3
    }
}