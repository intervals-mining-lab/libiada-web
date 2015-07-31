namespace LibiadaWeb.Models.Account
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    /// <summary>
    /// The configure two factor view model.
    /// </summary>
    public class ConfigureTwoFactorViewModel
    {
        /// <summary>
        /// Gets or sets the selected provider.
        /// </summary>
        public string SelectedProvider { get; set; }

        /// <summary>
        /// Gets or sets the providers.
        /// </summary>
        public ICollection<SelectListItem> Providers { get; set; }
    }
}
