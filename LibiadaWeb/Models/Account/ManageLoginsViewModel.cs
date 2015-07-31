namespace LibiadaWeb.Models.Account
{
    using System.Collections.Generic;

    using Microsoft.AspNet.Identity;
    using Microsoft.Owin.Security;

    /// <summary>
    /// The manage logins view model.
    /// </summary>
    public class ManageLoginsViewModel
    {
        /// <summary>
        /// Gets or sets the current logins.
        /// </summary>
        public IList<UserLoginInfo> CurrentLogins { get; set; }

        /// <summary>
        /// Gets or sets the other logins.
        /// </summary>
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }
}
