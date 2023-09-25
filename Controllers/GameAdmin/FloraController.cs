using NetMudCore.Authentication;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.NaturalResource;
using NetMudCore.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class FloraController : Controller
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

        public FloraController()
        {
        }

        public FloraController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageFloraViewModel vModel = new(TemplateCache.GetAll<IFlora>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Flora/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Flora/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public async Task<ActionResult> RemoveAsync(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IFlora obj = TemplateCache.Get<IFlora>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveFlora[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                IFlora obj = TemplateCache.Get<IFlora>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveFlora[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
        public async Task<ActionResult> AddAsync(long Template = -1)
        {
            AddEditFloraViewModel vModel = new(Template)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("~/Views/GameAdmin/Flora/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAsync(AddEditFloraViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IFlora newObj = vModel.DataObject;

            if (newObj.Wood == null && newObj.Flower == null && newObj.Seed == null && newObj.Leaf == null && newObj.Fruit == null)
            {
                message = "At least one of the parts of this plant must be valid.";
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
                {
                    message = "Error; Creation failed.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddFlora[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditAsync(long id, string ArchivePath = "")
        {
            IFlora obj = TemplateCache.Get<IFlora>(id);

            if (obj == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            AddEditFloraViewModel vModel = new(ArchivePath, obj)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("~/Views/GameAdmin/Flora/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(long id, AddEditFloraViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IFlora obj = TemplateCache.Get<IFlora>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.HelpText = vModel.DataObject.HelpText;
            obj.SunlightPreference = vModel.DataObject.SunlightPreference;
            obj.Coniferous = vModel.DataObject.Coniferous;
            obj.AmountMultiplier = vModel.DataObject.AmountMultiplier;
            obj.Rarity = vModel.DataObject.Rarity;
            obj.PuissanceVariance = vModel.DataObject.PuissanceVariance;
            obj.ElevationRange = vModel.DataObject.ElevationRange;
            obj.TemperatureRange = vModel.DataObject.TemperatureRange;
            obj.HumidityRange = vModel.DataObject.HumidityRange;
            obj.Wood = vModel.DataObject.Wood;
            obj.Flower = vModel.DataObject.Flower;
            obj.Seed = vModel.DataObject.Seed;
            obj.Leaf = vModel.DataObject.Leaf;
            obj.Fruit = vModel.DataObject.Fruit;
            obj.OccursIn = vModel.DataObject.OccursIn;

            if (obj.Wood == null && obj.Flower == null && obj.Seed == null && obj.Leaf == null && obj.Fruit == null)
            {
                message = "At least one of the parts of this plant must be valid.";
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditFlora[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                {
                    message = "Error; Edit failed.";
                }
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}