using NetMudCore.Authentication;
using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class MaterialController : Controller
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

        public MaterialController()
        {
        }

        public MaterialController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageMaterialDataViewModel vModel = new(TemplateCache.GetAll<Material>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Material/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Material/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public async Task<ActionResult> RemoveAsync(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IMaterial obj = TemplateCache.Get<IMaterial>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveMaterial[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                IMaterial obj = TemplateCache.Get<IMaterial>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveMaterial[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            AddEditMaterialViewModel vModel = new(Template)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("~/Views/GameAdmin/Material/Add.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAsync(AddEditMaterialViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IMaterial newObj = vModel.DataObject;
            string message;
            if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddMaterialData[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditAsync(int id, string ArchivePath = "")
        {
            Material obj = TemplateCache.Get<Material>(id);

            if (obj == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            AddEditMaterialViewModel vModel = new(ArchivePath, obj)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("~/Views/GameAdmin/Material/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(int id, AddEditMaterialViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            Material obj = TemplateCache.Get<Material>(id);
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Conductive = vModel.DataObject.Conductive;
            obj.Density = vModel.DataObject.Density;
            obj.Ductility = vModel.DataObject.Ductility;
            obj.Flammable = vModel.DataObject.Flammable;
            obj.GasPoint = vModel.DataObject.GasPoint;
            obj.Magnetic = vModel.DataObject.Magnetic;
            obj.Mallebility = vModel.DataObject.Mallebility;
            obj.Porosity = vModel.DataObject.Porosity;
            obj.SolidPoint = vModel.DataObject.SolidPoint;
            obj.TemperatureRetention = vModel.DataObject.TemperatureRetention;
            obj.Viscosity = vModel.DataObject.Viscosity;
            obj.HelpText = vModel.DataObject.HelpText;
            obj.Resistance = vModel.DataObject.Resistance;
            obj.Composition = vModel.DataObject.Composition;

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditMaterialData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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