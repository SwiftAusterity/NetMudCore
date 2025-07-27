using NetMudCore.Authentication;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class ContentApprovalController : Controller
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

        public ContentApprovalController()
        {
        }

        public ContentApprovalController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult> IndexAsync()
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (authedUser == null)
            {
                return BadRequest();
            }

            IOrderedEnumerable<IKeyedData> newList = TemplateCache.GetAll().Where(item => item.GetType().GetInterfaces().Contains(typeof(INeedApproval))
                                                                && item.GetType().GetInterfaces().Contains(typeof(IKeyedData))
                                                                && !item.SuitableForUse && item.CanIBeApprovedBy(ApplicationUser.GetStaffRank(User), authedUser.GameAccount)).OrderBy(item => item.GetType().Name);

            ManageContentApprovalsViewModel viewModel = new(newList);

            return View("~/Views/GameAdmin/ContentApproval/Index.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApproveAllAsync()
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (authedUser == null)
            {
                return BadRequest();
            }

            IOrderedEnumerable<IKeyedData> newList = TemplateCache.GetAll().Where(item => item.GetType().GetInterfaces().Contains(typeof(INeedApproval))
                                                                && item.GetType().GetInterfaces().Contains(typeof(IKeyedData))
                                                                && !item.SuitableForUse && item.CanIBeApprovedBy(ApplicationUser.GetStaffRank(User), authedUser.GameAccount)).OrderBy(item => item.GetType().Name);

            foreach(IKeyedData thing in newList)
            {
                thing.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Approved);

                LoggingUtility.LogAdminCommandUsage("*WEB* - Approve (all) Content[" + thing.Id + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return RedirectToAction("Index", new { Message = "All have been approved." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApproveDenyAsync(long? approvalId, string authorizeApproval, long? denialId, string authorizeDenial)
        {
            bool approve = true;

            string[] approvalIdSplit = authorizeApproval.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);
            string[] denialIdSplit = authorizeDenial.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);
            string message;
            if ((!string.IsNullOrWhiteSpace(authorizeApproval) && approvalIdSplit.Length > 0 && approvalId.ToString().Equals(approvalIdSplit[0])) ||
                (!string.IsNullOrWhiteSpace(authorizeDenial) && denialIdSplit.Length > 0 && denialId.ToString().Equals(denialIdSplit[0])))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                IKeyedData obj = null;

                if (!string.IsNullOrWhiteSpace(authorizeDenial) && denialIdSplit.Length > 0 && denialId.ToString().Equals(denialIdSplit[0]))
                {
                    Type type = Type.GetType(denialIdSplit[1]);

                    approve = false;
                    obj = (IKeyedData)TemplateCache.Get(new TemplateCacheKey(type, denialId.Value));
                }
                else if (!string.IsNullOrWhiteSpace(authorizeApproval) && approvalIdSplit.Length > 0 && approvalId.ToString().Equals(approvalIdSplit[0]))
                {
                    Type type = Type.GetType(approvalIdSplit[1]);

                    obj = (IKeyedData)TemplateCache.Get(new TemplateCacheKey(type, approvalId.Value));
                }

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else
                {
                    if (approve)
                    {
                        if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Approved))
                        {
                            message = "Approve Successful.";
                        }
                        else
                        {
                            message = "Error; Approve failed.";
                        }

                        LoggingUtility.LogAdminCommandUsage("*WEB* - Approve Content[" + authorizeApproval + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    }
                    else
                    {
                        if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Unapproved))
                        {
                            message = "Deny Successful.";
                        }
                        else
                        {
                            message = "Error; Deny failed.";
                        }

                        LoggingUtility.LogAdminCommandUsage("*WEB* - Deny Content[" + authorizeDenial + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    }
                }
            }
            else
            {
                message = "You must check the proper radio button first.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}