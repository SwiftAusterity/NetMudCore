using NetMudCore.Authentication;
using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class DimensionalModelController : Controller
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

        public DimensionalModelController()
        {
        }

        public DimensionalModelController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageDimensionalModelDataViewModel vModel = new(TemplateCache.GetAll<DimensionalModelData>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/DimensionalModel/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"DimensionalModel/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public async Task<ActionResult> RemoveAsync(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IDimensionalModelData obj = TemplateCache.Get<IDimensionalModelData>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveDimensionalModelData[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                IDimensionalModelData obj = TemplateCache.Get<IDimensionalModelData>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveDimensionalModelData[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            AddEditDimensionalModelDataViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = new DimensionalModelData()
            };

            return View("~/Views/GameAdmin/DimensionalModel/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAsync(AddEditDimensionalModelDataViewModel vModel, HttpPostedFileBase modelFile)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            string message;
            try
            {
                IDimensionalModelData newModel = vModel.DataObject;

                foreach (IDimensionalModelPlane plane in newModel.ModelPlanes)
                {
                    foreach (IDimensionalModelNode node in plane.ModelNodes)
                    {
                        node.YAxis = plane.YAxis;
                    }
                }

                if (newModel.IsModelValid())
                {
                    if (newModel.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
                    {
                        message = "Error; Creation failed.";
                    }
                    else
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - AddDimensionalModelData[" + newModel.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Creation Successful.";
                    }
                }
                else
                {
                    message = "Invalid model file; Model files must contain 21 planes of a tag name followed by 21 rows of 21 nodes.";
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }


        [HttpGet]
        public async Task<ActionResult> EditAsync(long id)
        {
            AddEditDimensionalModelDataViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            IDimensionalModelData obj = TemplateCache.Get<IDimensionalModelData>(id);

            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;

            return View("~/Views/GameAdmin/DimensionalModel/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(long id, AddEditDimensionalModelDataViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IDimensionalModelData obj = TemplateCache.Get<IDimensionalModelData>(id);
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            try
            {
                foreach (IDimensionalModelPlane plane in vModel.DataObject.ModelPlanes)
                {
                    foreach (IDimensionalModelNode node in plane.ModelNodes)
                    {
                        node.YAxis = plane.YAxis;
                    }
                }

                if (vModel.DataObject.IsModelValid())
                {
                    obj.Name = vModel.DataObject.Name;
                    obj.ModelType = vModel.DataObject.ModelType;
                    obj.ModelPlanes = vModel.DataObject.ModelPlanes;
                    obj.Vacuity = vModel.DataObject.Vacuity;

                    if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - EditDimensionalModelData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Edit Successful.";
                    }
                    else
                    {
                        message = "Error; Edit failed.";
                    }
                }
                else
                {
                    message = "Invalid model; Models must contain 21 planes of a tag name followed by 21 rows of 21 nodes.";
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}