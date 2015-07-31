namespace LibiadaWeb.Models.Account
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The add phone number view model.
    /// </summary>
    public class AddPhoneNumberViewModel
    {
        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }
}
