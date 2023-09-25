using NetMudCore.Authentication;
using NetMudCore.DataAccess;
using NetMudCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Authorize]
    public class GameClientController : Controller
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

        public GameClientController()
        {
        }

        public GameClientController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync()
        {
            GameContextModel model = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            model.MusicTracks = ContentUtility.GetMusicTracksForZone(model.AuthedUser.GameAccount.GetCurrentlySelectedCharacter()?.CurrentLocation?.CurrentZone);
            model.MusicPlaylists = model.AuthedUser.GameAccount.Config.Playlists;
            return View(model);
        }

        [HttpGet]
        public ActionResult ModularWindow()
        {
            return View("~/Views/GameClient/ModularWindow.cshtml");
        }
    }
}