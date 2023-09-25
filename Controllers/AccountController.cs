using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetMudCore.Authentication;
using NetMudCore.DataStructure.System;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.Models.AccountManagement;
using NetMudCore.Data.Players;
using NetMudCore.DataStructure.Players;

namespace NetMudCore.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser>? _userManager;
        public UserManager<ApplicationUser> UserManager
        {
            get
            {
                return _userManager ?? HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private SignInManager<ApplicationUser>? signInManager;
        public SignInManager<ApplicationUser> SignInManager
        {
            get
            {
                return signInManager ?? HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
            }
            private set
            {
                signInManager = value;
            }
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(model: new LoginViewModel(string.Empty, string.Empty));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            SignInManager?.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));
            RegisterViewModel vModel = new(string.Empty, string.Empty, string.Empty, string.Empty);

            if (globalConfig.UserCreationActive)
            {
                return View(vModel);
            }

            ModelState.AddModelError("", "New account registration is currently locked.");
            vModel.NewUserLocked = true;

            return View(vModel);
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code) || UserManager == null)
            {
                return View("Error");
            }

            var user = await UserManager.FindByNameAsync(userId);
            if (user == null)
            {
                return View("Error");
            }

            IdentityResult result = await UserManager.ConfirmEmailAsync(user, code);

            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid || UserManager == null || SignInManager == null)
            {
                return View(model);
            }

            ApplicationUser? potentialUser = await UserManager.FindByNameAsync(model.Email);

            if (potentialUser != null)
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));
                if (globalConfig.AdminsOnly && ApplicationUser.GetStaffRank(User) == StaffRank.Player)
                {
                    ModelState.AddModelError("", "The system is currently locked to staff members only. Please try again later and check the home page for any announcements and news.");

                    return View(model);
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                Microsoft.AspNetCore.Identity.SignInResult result = await SignInManager.PasswordSignInAsync(potentialUser, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    //Check for a valid character, zone and account
                    Account account = potentialUser.GameAccount;

                    if (account == null)
                    {
                        ModelState.AddModelError("", "Your account is having technical difficulties. Please contact an administrator.");
                        return View(model);
                    }

                    return RedirectToLocal(returnUrl);
                }
                else if (result.IsLockedOut)
                {
                    return View("Lockout");
                }
                else if (result.RequiresTwoFactor)
                {
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe });
                }
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if ( UserManager == null || SignInManager == null)
            {
                return View(model);
            }

            //dupe handles
            if (Account.GetByHandle(model.GlobalUserHandle) != null)
            {
                ModelState.AddModelError("GlobalUserHandle", "That handle already exists in the system. Please choose another.");
            }

            if (ModelState.IsValid)
            {
                Account newGameAccount = new(model.GlobalUserHandle);

                ApplicationUser user = new()
                {
                    UserName = model.Email,
                    Email = model.Email,
                    GameAccount = newGameAccount,
                    GlobalIdentityHandle = newGameAccount.GlobalIdentityHandle
                };

                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    CreateAccountPlayerAndConfig(newGameAccount);

                    await UserManager.AddToRoleAsync(user, "Player");
                    await SignInManager.SignInAsync(user, isPersistent: false);
                    IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                    string callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Scheme) ?? string.Empty;
                    EmailUtility.SendEmail(globalConfig, user, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid && UserManager != null)
            {
                ApplicationUser? user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user);
                string callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code }, protocol: Request.Scheme) ?? string.Empty;
                EmailUtility.SendEmail(globalConfig, user, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid || UserManager == null)
            {
                return View(model);
            }
            ApplicationUser? user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            IdentityResult result = await UserManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        private static void CreateAccountPlayerAndConfig(IAccount account)
        {
            AccountConfig newAccountConfig = new(account);

            IEnumerable<IUIModule> uiModules = TemplateCache.GetAll<IUIModule>().Where(uim => uim.SystemDefault > 0);

            foreach (IUIModule module in uiModules)
            {
                newAccountConfig.UIModules = uiModules.Select(uim => new Tuple<IUIModule, int>(uim, uim.SystemDefault));
            }

            //Save the new config
            newAccountConfig.Save(account, StaffRank.Player);
        }
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", string.Format("{0} ({1})", error.Description, error.Code));
            }
        }
    }
}
