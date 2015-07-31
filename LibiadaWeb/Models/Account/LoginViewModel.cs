namespace LibiadaWeb.Models.Account
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The login view model.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether remember me.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
