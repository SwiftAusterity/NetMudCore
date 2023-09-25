using NetMudCore.Authentication;
using NetMudCore.Cartography;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Players;
using NetMudCore.DataStructure.Room;
using NetMudCore.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using AlloyTemplates;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace NetMudCore.Controllers
{
    [Authorize]
    public class ClientDataApiController : ApiController
    {
        private UserManager<ApplicationUser>? _userManager;
        public UserManager<ApplicationUser> UserManager
        {
            get
            {
                return _userManager ?? HttpContextHelper.Current.GetOwinContext().GetUserManager<UserManager<ApplicationUser>>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpPost]
        [Route("api/ClientDataApi/ToggleTutorialMode", Name = "ClientDataAPI_ToggleTutorialMode")]
        public async Task<JsonResult<bool>> ToggleTutorialModeAsync()
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if(user != null)
            {
                user.GameAccount.Config.UITutorialMode = !user.GameAccount.Config.UITutorialMode;
                user.GameAccount.Config.Save(user.GameAccount, StaffRank.Admin);
            }

            return Json(user.GameAccount.Config.UITutorialMode);
        }

        [HttpPost]
        [Route("api/ClientDataApi/ToggleTutorialMode/{language}", Name = "ClientDataAPI_ChangeLanguage")]
        public async Task<JsonResult<bool>> ChangeUILanguageAsync(string language)
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            ILanguage lang = ConfigDataCache.Get<ILanguage>(new ConfigDataCacheKey(typeof(ILanguage), language, ConfigDataType.Language));

            if (user != null && lang != null)
            {
                user.GameAccount.Config.UILanguage = lang;
                user.GameAccount.Config.Save(user.GameAccount, StaffRank.Admin);
            }

            return Json(user.GameAccount.Config.UITutorialMode);
        }

        [HttpGet]
        public string GetEntityModelView(long modelId)
        {
            IDimensionalModelData model = TemplateCache.Get<IDimensionalModelData>(modelId);

            if (model == null)
            {
                return string.Empty;
            }

            return Render.FlattenModelForWeb(model);
        }

        [HttpGet]
        public string RenderRoomWithRadius(long id, int radius)
        {
            IRoomTemplate centerRoom = TemplateCache.Get<IRoomTemplate>(id);

            if (centerRoom == null || radius < 0)
            {
                return "Invalid inputs.";
            }

            return Rendering.RenderRadiusMap(centerRoom, radius, false);
        }

        [HttpGet]
        public JsonResult<IUIModule> GetUIModuleContent(string moduleName)
        {
            IUIModule module = TemplateCache.GetByName<IUIModule>(moduleName);

            if (module != null)
            {
                return Json(module);
            }

            return null;
        }

        [HttpPost]
        [Route("api/ClientDataApi/ToggleSoundMute", Name = "ClientDataAPI_ToggleSoundMute")]
        public async Task<JsonResult<bool>> ToggleSoundMuteAsync()
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (user != null)
            {
                user.GameAccount.Config.SoundMuted = !user.GameAccount.Config.SoundMuted;
                user.GameAccount.Config.Save(user.GameAccount, StaffRank.Admin);
            }

            return Json(user.GameAccount.Config.SoundMuted);
        }

        [HttpPost]
        [Route("api/ClientDataApi/ToggleMusicMute", Name = "ClientDataAPI_ToggleMusicMute")]
        public async Task<JsonResult<bool>> ToggleMusicMuteAsync()
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (user != null)
            {
                user.GameAccount.Config.MusicMuted = !user.GameAccount.Config.MusicMuted;
                user.GameAccount.Config.Save(user.GameAccount, StaffRank.Admin);
            }

            return Json(user.GameAccount.Config.MusicMuted);
        }

        [HttpPost]
        [Route("api/ClientDataApi/ToggleGossipParticipation", Name = "ClientDataAPI_ToggleGossipParticipation")]
        public async Task<JsonResult<bool>> ToggleGossipParticipationAsync()
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (user != null)
            {
                user.GameAccount.Config.GossipSubscriber = !user.GameAccount.Config.GossipSubscriber;
                user.GameAccount.Config.Save(user.GameAccount, StaffRank.Admin);
            }

            return Json(user.GameAccount.Config.GossipSubscriber);
        }
		
        [HttpPost]
        public async Task<string> RemoveUIModuleContentAsync(string moduleName, int location)
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            Data.Players.Account account = user.GameAccount;

            if (account == null)
            {
                return "Invalid Account.";
            }

            List<Tuple<IUIModule, int>> modules = account.Config.UIModules.ToList();
            if (moduleName == "**anymodule**" && location != -1)
            {
                if (modules.Any(mod => mod.Item2.Equals(location)))
                {
                    Tuple<IUIModule, int> moduleTuple = modules.FirstOrDefault(mod => mod.Item2.Equals(location));
                    modules.Remove(moduleTuple);
                }
            }
            else
            {
                IUIModule module = TemplateCache.GetByName<IUIModule>(moduleName);

                if (module == null)
                {
                    return "Invalid Module.";
                }

                if ((location < 1 && location != -1) || location > 4)
                {
                    return "Invalid Location";
                }

                Tuple<IUIModule, int> moduleTuple = new(module, location);

                //Remove this module
                if (modules.Any(mod => mod.Item1.Equals(module) && mod.Item2.Equals(location)))
                {
                    modules.Remove(moduleTuple);
                }
            }

            account.Config.UIModules = modules;
            account.Config.Save(account, StaffRank.Player);

            return "Success";
        }

        [HttpPost]
        public async Task<string> SaveUIModuleContentAsync(string moduleName, int location)
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            Data.Players.Account account = user.GameAccount;

            if (account == null)
            {
                return "Invalid Account.";
            }

            IUIModule module = TemplateCache.GetByName<IUIModule>(moduleName);

            if (module == null)
            {
                return "Invalid Module.";
            }

            if ((location < 1 && location != -1) || location > 4)
            {
                return "Invalid Location";
            }

            List<Tuple<IUIModule, int>> modules = account.Config.UIModules.ToList();
            Tuple<IUIModule, int> moduleTuple = new(module, location);

            //Remove this module
            if (modules.Any(mod => mod.Item1.Equals(module)))
            {
                modules.Remove(moduleTuple);
            }

            //Remove the module in its place
            if (location != -1 && modules.Any(mod => mod.Item2.Equals(location)))
            {
                modules.RemoveAll(mod => mod.Item2.Equals(location));
            }

            //Add it finally
            modules.Add(moduleTuple);

            account.Config.UIModules = modules;

            account.Config.Save(account, StaffRank.Player);

            return "Success";
        }

        [HttpGet]
        public async Task<JsonResult<IEnumerable<Tuple<IUIModule, int>>>> LoadUIModulesAsync()
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            Data.Players.Account account = user.GameAccount;

            if (account == null)
            {
                return null;
            }

            return Json(account.Config.UIModules);
        }

        [HttpGet]
        [Route("api/ClientDataApi/GetUIModuleNames", Name = "ClientDataAPI_GetUIModuleNames")]
        public async Task<JsonResult<string[]>> GetUIModuleNamesAsync(string term)
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            Data.Players.Account account = user.GameAccount;

            if (account == null)
            {
                return Json(Array.Empty<string>());
            }

            IEnumerable<IUIModule> modules = TemplateCache.GetAll<IUIModule>(true).Where(uim => uim.Name.Contains(term));

            return Json(modules.Select(mod => mod.Name).ToArray());
        }

        [HttpGet]
        [Route("api/ClientDataApi/GetAccountNames", Name = "ClientDataAPI_GetAccountNames")]
        public JsonResult<string[]> GetAccountNames(string term)
        {
            IQueryable<ApplicationUser> accounts = UserManager.Users;

            return Json(accounts.Where(acct => acct.GlobalIdentityHandle.Contains(term)).Select(acct => acct.GlobalIdentityHandle).ToArray());
        }

        [HttpGet]
        [Route("api/ClientDataApi/GetCharacterNamesForAccount/{accountName}", Name = "ClientDataAPI_GetCharacterNamesForAccount")]
        public async Task<JsonResult<string[]>> GetCharacterNamesForAccountAsync(string accountName, string term)
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IEnumerable<IPlayerTemplate> characters = PlayerDataCache.GetAll().Where(chr => chr.AccountHandle.Equals(user.GlobalIdentityHandle) && chr.Name.Contains(term));

            return Json(characters.Select(chr => chr.Name).ToArray());
        }
    }
}
