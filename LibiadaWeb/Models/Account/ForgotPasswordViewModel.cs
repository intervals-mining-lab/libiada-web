namespace LibiadaWeb.Models.Account
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The forgot password view model.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
