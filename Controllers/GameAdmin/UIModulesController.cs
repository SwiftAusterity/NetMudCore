using NetMudCore.Authentication;
using NetMudCore.Data.Player;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Players;
using NetMudCore.Models.Admin;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class UIModulesController : Controller
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

        public UIModulesController()
        {
        }

        public UIModulesController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageUIModulesViewModel vModel = new(TemplateCache.GetAll<IUIModule>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/UIModules/Index.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"UIModules/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public async Task<ActionResult> RemoveAsync(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IUIModule obj = TemplateCache.Get<IUIModule>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveUIModule[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                {
                    message = "Error; Removal failed.";
                }
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IUIModule obj = TemplateCache.Get<IUIModule>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveUIModule[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Unapproval Successful.";
                }
                else
                {
                    message = "Error; Unapproval failed.";
                }
            }
            else
            {
                message = "You must check the proper remove or unapprove authorization radio button first.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> AddAsync()
        {
            AddEditUIModuleViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("~/Views/GameAdmin/UIModules/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAsync(AddEditUIModuleViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            UIModule newObj = new()
            {
                Name = vModel.Name,
                BodyHtml = vModel.BodyHtml,
                Height = vModel.Height,
                Width = vModel.Width,
                HelpText = vModel.HelpText,
                SystemDefault = vModel.SystemDefault
            };

            IEnumerable<IUIModule> uiModules = TemplateCache.GetAll<IUIModule>().Where(uim => vModel.SystemDefault > 0 && uim.SystemDefault == vModel.SystemDefault);

            if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                if (uiModules.Any())
                {
                    IUIModule revertModule = uiModules.First();
                    revertModule.SystemDefault = 0;
                    revertModule.Save(authedUser.GameAccount, StaffRank.Admin);
                }

                LoggingUtility.LogAdminCommandUsage("*WEB* - AddUIModule[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditAsync(long id)
        {
            AddEditUIModuleViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            IUIModule obj = TemplateCache.Get<IUIModule>(id);

            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.BodyHtml = obj.BodyHtml.Value;
            vModel.Height = obj.Height;
            vModel.Width = obj.Width;
            vModel.HelpText = obj.HelpText.Value;
            vModel.SystemDefault = obj.SystemDefault;

            return View("~/Views/GameAdmin/UIModules/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(long id, AddEditUIModuleViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IUIModule obj = TemplateCache.Get<IUIModule>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;
            obj.BodyHtml = vModel.BodyHtml;
            obj.Height = vModel.Height;
            obj.Width = vModel.Width;
            obj.HelpText = vModel.HelpText;
            obj.SystemDefault = vModel.SystemDefault;

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                IEnumerable<IUIModule> uiModules = TemplateCache.GetAll<IUIModule>().Where(uim => vModel.SystemDefault > 0 
                    && uim.SystemDefault == vModel.SystemDefault && uim != obj);

                if (uiModules.Any())
                {
                    IUIModule revertModule = uiModules.First();
                    revertModule.SystemDefault = 0;
                    revertModule.Save(authedUser.GameAccount, StaffRank.Admin);
                }

                LoggingUtility.LogAdminCommandUsage("*WEB* - EditUIModule[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}