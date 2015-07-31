namespace LibiadaWeb.Models.Account
{
    using System.Collections.Generic;

    /// <summary>
    /// The send code view model.
    /// </summary>
    public class SendCodeViewModel
    {
        /// <summary>
        /// Gets or sets the selected provider.
        /// </summary>
        public string SelectedProvider { get; set; }

        /// <summary>
        /// Gets or sets the providers.
        /// </summary>
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }

        /// <summary>
        /// Gets or sets the return url.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether remember me.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
