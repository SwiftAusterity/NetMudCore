using NetMudCore.Authentication;
using NetMudCore.DataAccess;
using NetMudCore.Models.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace NetMudCore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoggingController : Controller
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

        public LoggingController()
        {
        }

        public LoggingController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string selectedLog)
        {
            DashboardViewModel dashboardModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                ChannelNames = LoggingUtility.GetCurrentLogNames()
            };

            if (!string.IsNullOrWhiteSpace(selectedLog))
            {
                dashboardModel.SelectedLogContent = LoggingUtility.GetCurrentLogContent(selectedLog);
                dashboardModel.SelectedLog = selectedLog;
            }

            return View(dashboardModel);
        }

        [HttpPost]
        public async Task<ActionResult> RolloverAsync(string selectedLog)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            string message;
            if (!string.IsNullOrWhiteSpace(selectedLog))
            {
                if (!LoggingUtility.RolloverLog(selectedLog))
                {
                    message = "Error rolling over log.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RolloverLog[" + selectedLog + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Rollover Successful.";
                }
            }
            else
            {
                message = "No log selected to rollover";
            }

            return RedirectToAction("Index", new { Message = message });

        }
    }
}