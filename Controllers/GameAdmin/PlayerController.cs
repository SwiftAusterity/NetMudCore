using NetMudCore.Authentication;
using NetMudCore.Data.Players;
using NetMudCore.DataAccess;
using NetMudCore.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize]
    public class PlayerController : Controller
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

        public PlayerController()
        {
        }

        public PlayerController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }


        [HttpPost]
        [Route(@"Player/SelectCharacter/{id}")]
        public async Task<JsonResult> SelectCharacterAsync(long id)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (authedUser != null && id >= 0)
            {
                authedUser.GameAccount.CurrentlySelectedCharacter = id;
                _ = UserManager.UpdateAsync(authedUser);
            }

            return Json(true);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> IndexAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            RoleManager<IdentityRole> roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();

            ManagePlayersViewModel vModel = new(UserManager.Users)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms,

                ValidRoles = roleManager.Roles.ToList()
            };

            return View("~/Views/GameAdmin/Player/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles="Admin")]
        [Route(@"Player/Remove/{removeId?}/{authorizeRemove?}")]
        public async Task<ActionResult> RemoveAsync(string removeId, string authorizeRemove)
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                if(authedUser == null)
                {
                    message = "Error; Removal failed.";
                    return RedirectToAction("Index", new { Message = message });
                }

                DataStructure.Players.IAccount obj = Account.GetByHandle(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Delete(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveAccount[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                {
                    message = "Error; Removal failed.";
                }
            }
            else
            {
                message = "You must check the proper remove or unapprove authorization radio button first.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}