namespace LibiadaWeb.Models.Account
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The external login confirmation view model.
    /// </summary>
    public class ExternalLoginConfirmationViewModel
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
