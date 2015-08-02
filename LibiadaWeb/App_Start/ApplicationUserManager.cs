namespace LibiadaWeb
{
    using System;

    using LibiadaWeb.Models.Account;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;

    /// <summary>
    /// The application user manager.
    /// </summary>
    public class ApplicationUserManager : UserManager<ApplicationUser, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserManager"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public ApplicationUserManager(IUserStore<ApplicationUser, int> store) : base(store)
        {
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="ApplicationUserManager"/>.
        /// </returns>
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new LibiadaUserStore(context.Get<ApplicationDbContext>()));
            
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser, int>(manager)
                                        {
                                            AllowOnlyAlphanumericUserNames = false,
                                            RequireUniqueEmail = true
                                        };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
                                            {
                                                RequiredLength = 6,
                                                RequireNonLetterOrDigit = false,
                                                RequireDigit = true,
                                                RequireLowercase = false,
                                                RequireUppercase = false,
                                            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser, int> { MessageFormat = "Your security code is {0}" });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser, int> { Subject = "Security Code", BodyFormat = "Your security code is {0}" });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser, int>(dataProtectionProvider.Create("ASP.NET Identity"));
            }

            return manager;
        }
    }
}
