// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountController.cs" company="">
//   
// </copyright>
// <summary>
//   The account controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using LibiadaWeb.Models;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;

namespace LibiadaWeb.Controllers
{
    /// <summary>
    /// The account controller.
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="userManager">
        /// The user manager.
        /// </param>
        public AccountController(UserManager<ApplicationUser> userManager)
        {
            this.UserManager = userManager;
        }

        /// <summary>
        /// Gets the user manager.
        /// </summary>
        public UserManager<ApplicationUser> UserManager { get; private set; }

        // GET: /Account/Login
        /// <summary>
        /// The login.
        /// </summary>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            this.ViewBag.ReturnUrl = returnUrl;
            return this.View();
        }

        // POST: /Account/Login
        /// <summary>
        /// The login.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this.UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await this.SignInAsync(user, model.RememberMe);
                    return this.RedirectToLocal(returnUrl);
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, "Invalid username or password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/Register
        /// <summary>
        /// The register.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public ActionResult Register()
        {
            return this.View();
        }

        // POST: /Account/Register
        /// <summary>
        /// The register.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = new ApplicationUser() { UserName = model.UserName };
                var result = await this.UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await this.SignInAsync(user, isPersistent: false);
                    return this.RedirectToAction("Index", "Home");
                }
                else
                {
                    this.AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // POST: /Account/Disassociate
        /// <summary>
        /// The disassociate.
        /// </summary>
        /// <param name="loginProvider">
        /// The login provider.
        /// </param>
        /// <param name="providerKey">
        /// The provider key.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result =
                await
                this.UserManager.RemoveLoginAsync(
                    this.User.Identity.GetUserId(), 
                    new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }

            return this.RedirectToAction("Manage", new { Message = message });
        }

        // GET: /Account/Manage
        /// <summary>
        /// The manage.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Manage(ManageMessageId? message)
        {
            this.ViewBag.StatusMessage = message == ManageMessageId.ChangePasswordSuccess
                                             ? "Your password has been changed."
                                             : message == ManageMessageId.SetPasswordSuccess
                                                   ? "Your password has been set."
                                                   : message == ManageMessageId.RemoveLoginSuccess
                                                         ? "The external login was removed."
                                                         : message == ManageMessageId.Error
                                                               ? "An error has occurred."
                                                               : string.Empty;
            this.ViewBag.HasLocalPassword = this.HasPassword();
            this.ViewBag.ReturnUrl = this.Url.Action("Manage");
            return this.View();
        }

        // POST: /Account/Manage
        /// <summary>
        /// The manage.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = this.HasPassword();
            this.ViewBag.HasLocalPassword = hasPassword;
            this.ViewBag.ReturnUrl = this.Url.Action("Manage");
            if (hasPassword)
            {
                if (this.ModelState.IsValid)
                {
                    IdentityResult result =
                        await
                        this.UserManager.ChangePasswordAsync(
                            this.User.Identity.GetUserId(), 
                            model.OldPassword, 
                            model.NewPassword);
                    if (result.Succeeded)
                    {
                        return this.RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        this.AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = this.ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (this.ModelState.IsValid)
                {
                    IdentityResult result =
                        await this.UserManager.AddPasswordAsync(this.User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return this.RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        this.AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // POST: /Account/ExternalLogin
        /// <summary>
        /// The external login.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(
                provider, 
                this.Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        // GET: /Account/ExternalLoginCallback
        /// <summary>
        /// The external login callback.
        /// </summary>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await this.AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return this.RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await this.UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await this.SignInAsync(user, isPersistent: false);
                return this.RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                this.ViewBag.ReturnUrl = returnUrl;
                this.ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return this.View(
                    "ExternalLoginConfirmation", 
                    new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        // POST: /Account/LinkLogin
        /// <summary>
        /// The link login.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(
                provider, 
                this.Url.Action("LinkLoginCallback", "Account"), 
                this.User.Identity.GetUserId());
        }

        // GET: /Account/LinkLoginCallback
        /// <summary>
        /// The link login callback.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo =
                await this.AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, this.User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return this.RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }

            var result = await this.UserManager.AddLoginAsync(this.User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return this.RedirectToAction("Manage");
            }

            return this.RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        // POST: /Account/ExternalLoginConfirmation
        /// <summary>
        /// The external login confirmation.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(
            ExternalLoginConfirmationViewModel model, 
            string returnUrl)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return this.RedirectToAction("Manage");
            }

            if (this.ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await this.AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return this.View("ExternalLoginFailure");
                }

                var user = new ApplicationUser() { UserName = model.UserName };
                var result = await this.UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await this.UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await this.SignInAsync(user, isPersistent: false);
                        return this.RedirectToLocal(returnUrl);
                    }
                }

                this.AddErrors(result);
            }

            this.ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // POST: /Account/LogOff
        /// <summary>
        /// The log off.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            this.AuthenticationManager.SignOut();
            return this.RedirectToAction("Index", "Home");
        }

        // GET: /Account/ExternalLoginFailure
        /// <summary>
        /// The external login failure.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return this.View();
        }

        /// <summary>
        /// The remove account list.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = this.UserManager.GetLogins(this.User.Identity.GetUserId());
            this.ViewBag.ShowRemoveButton = this.HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)this.PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.UserManager != null)
            {
                this.UserManager.Dispose();
                this.UserManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        /// <summary>
        /// The xsrf key.
        /// </summary>
        private const string XsrfKey = "XsrfId";

        /// <summary>
        /// Gets the authentication manager.
        /// </summary>
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return this.HttpContext.GetOwinContext().Authentication;
            }
        }

        /// <summary>
        /// The sign in async.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="isPersistent">
        /// The is persistent.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            this.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity =
                await this.UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            this.AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        /// <summary>
        /// The add errors.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error);
            }
        }

        /// <summary>
        /// The has password.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool HasPassword()
        {
            var user = this.UserManager.FindById(this.User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }

            return false;
        }

        /// <summary>
        /// The manage message id.
        /// </summary>
        public enum ManageMessageId
        {
            /// <summary>
            /// The change password success.
            /// </summary>
            ChangePasswordSuccess, 

            /// <summary>
            /// The set password success.
            /// </summary>
            SetPasswordSuccess, 

            /// <summary>
            /// The remove login success.
            /// </summary>
            RemoveLoginSuccess, 

            /// <summary>
            /// The error.
            /// </summary>
            Error
        }

        /// <summary>
        /// The redirect to local.
        /// </summary>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// The challenge result.
        /// </summary>
        private class ChallengeResult : HttpUnauthorizedResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ChallengeResult"/> class.
            /// </summary>
            /// <param name="provider">
            /// The provider.
            /// </param>
            /// <param name="redirectUri">
            /// The redirect uri.
            /// </param>
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ChallengeResult"/> class.
            /// </summary>
            /// <param name="provider">
            /// The provider.
            /// </param>
            /// <param name="redirectUri">
            /// The redirect uri.
            /// </param>
            /// <param name="userId">
            /// The user id.
            /// </param>
            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                this.LoginProvider = provider;
                this.RedirectUri = redirectUri;
                this.UserId = userId;
            }

            /// <summary>
            /// Gets or sets the login provider.
            /// </summary>
            public string LoginProvider { get; set; }

            /// <summary>
            /// Gets or sets the redirect uri.
            /// </summary>
            public string RedirectUri { get; set; }

            /// <summary>
            /// Gets or sets the user id.
            /// </summary>
            public string UserId { get; set; }

            /// <summary>
            /// The execute result.
            /// </summary>
            /// <param name="context">
            /// The context.
            /// </param>
            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = this.RedirectUri };
                if (this.UserId != null)
                {
                    properties.Dictionary[XsrfKey] = this.UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, this.LoginProvider);
            }
        }

        #endregion
    }
}