using NetMudCore.Authentication;
using NetMudCore.Data.Combat;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Combat;
using NetMudCore.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class FightingArtController : Controller
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

        public FightingArtController()
        {
        }

        public FightingArtController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageFightingArtViewModel vModel = new(TemplateCache.GetAll<IFightingArt>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/FightingArt/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"FightingArt/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public async Task<ActionResult> RemoveAsync(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IFightingArt obj = TemplateCache.Get<IFightingArt>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveRoom[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                IFightingArt obj = TemplateCache.Get<IFightingArt>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveRoom[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            AddEditFightingArtViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = new FightingArt()
            };

            return View("~/Views/GameAdmin/FightingArt/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAsync(AddEditFightingArtViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IFightingArt newObj = vModel.DataObject;

            if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddFightingArt[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditAsync(int id)
        {
            IFightingArt obj = TemplateCache.Get<IFightingArt>(id);

            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToRoute("ErrorOrClose", new { Message = message });
            }

            AddEditFightingArtViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = obj,
            };

            return View("~/Views/GameAdmin/FightingArt/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(int id, AddEditFightingArtViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IFightingArt obj = TemplateCache.Get<IFightingArt>(id);
            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToRoute("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.ActorCriteria = vModel.DataObject.ActorCriteria;
            obj.Aim = vModel.DataObject.Aim;
            obj.Armor = vModel.DataObject.Armor;
            obj.DistanceChange = vModel.DataObject.DistanceChange;
            obj.DistanceRange = vModel.DataObject.DistanceRange;
            obj.Health = vModel.DataObject.Health;
            obj.HelpText = vModel.DataObject.HelpText;
            obj.Impact = vModel.DataObject.Impact;
            obj.PositionResult = vModel.DataObject.PositionResult;
            obj.Recovery = vModel.DataObject.Recovery;
            obj.RekkaKey = vModel.DataObject.RekkaKey;
            obj.RekkaPosition = vModel.DataObject.RekkaPosition;
            obj.Setup = vModel.DataObject.Setup;
            obj.Stamina = vModel.DataObject.Stamina;
            obj.VictimCriteria = vModel.DataObject.VictimCriteria;
            obj.ResultQuality = vModel.DataObject.ResultQuality;
            obj.AdditiveQuality = vModel.DataObject.AdditiveQuality;
            obj.QualityValue = vModel.DataObject.QualityValue;
            obj.Readiness = vModel.DataObject.Readiness;
            obj.ActionVerb = vModel.DataObject.ActionVerb;
            obj.ActionSubject = vModel.DataObject.ActionSubject;
            obj.ActionPredicate = vModel.DataObject.ActionPredicate;


            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditFightingArt[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
            }

            return RedirectToAction("Index");
        }
    }
}