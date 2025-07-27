using NetMudCore.Authentication;
using NetMudCore.Backup;
using NetMudCore.CentralControl;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Combat;
using NetMudCore.DataStructure.Gaia;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Locale;
using NetMudCore.DataStructure.NaturalResource;
using NetMudCore.DataStructure.NPC;
using NetMudCore.DataStructure.Players;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.System;
using NetMudCore.DataStructure.Zone;
using NetMudCore.Models.Admin;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class GameAdminController : Controller
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

        public GameAdminController()
        {
        }

        public GameAdminController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        //Also called Dashboard in most of the html
        public async Task<ActionResult> IndexAsync()
        {
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));
            //IGossipConfig gossipConfig = ConfigDataCache.Get<IGossipConfig>(new ConfigDataCacheKey(typeof(IGossipConfig), "GossipSettings", ConfigDataType.GameWorld));

            DashboardViewModel dashboardModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                Inanimates = TemplateCache.GetAll<IInanimateTemplate>(),
                NPCs = TemplateCache.GetAll<INonPlayerCharacterTemplate>(),
                Zones = TemplateCache.GetAll<IZoneTemplate>(),
                Worlds = TemplateCache.GetAll<IGaiaTemplate>(),
                Locales = TemplateCache.GetAll<ILocaleTemplate>(),
                Rooms = TemplateCache.GetAll<IRoomTemplate>(),

                HelpFiles = TemplateCache.GetAll<IHelp>(),
                Races = TemplateCache.GetAll<IRace>(),
                Celestials = TemplateCache.GetAll<ICelestial>(),
                Journals = TemplateCache.GetAll<IJournalEntry>(),
                DimensionalModels = TemplateCache.GetAll<IDimensionalModelData>(),
                Flora = TemplateCache.GetAll<IFlora>(),
                Fauna = TemplateCache.GetAll<IFauna>(),
                Minerals = TemplateCache.GetAll<IMineral>(),
                Materials = TemplateCache.GetAll<IMaterial>(),
                DictionaryWords = ConfigDataCache.GetAll<ILexeme>(),
                Languages = ConfigDataCache.GetAll<ILanguage>(),
                Genders = TemplateCache.GetAll<IGender>(),
                UIModules = ConfigDataCache.GetAll<IUIModule>(),
                FightingArts = TemplateCache.GetAll<IFightingArt>(),

                LiveTaskTokens = Processor.GetAllLiveTaskStatusTokens(),
                LivePlayers = LiveCache.GetAll<IPlayer>().Count(),
                LiveInanimates = LiveCache.GetAll<IInanimate>().Count(),
                LiveNPCs = LiveCache.GetAll<INonPlayerCharacter>().Count(),
                LiveZones = LiveCache.GetAll<IZone>().Count(),
                LiveWorlds = LiveCache.GetAll<IGaia>().Count(),
                LiveLocales = LiveCache.GetAll<ILocale>().Count(),
                LiveRooms = LiveCache.GetAll<IRoom>().Count(),

                ConfigDataObject = globalConfig,
                WebsocketPortalActive = globalConfig.WebsocketPortalActive,
                AdminsOnly = globalConfig.AdminsOnly,
                UserCreationActive = globalConfig.UserCreationActive,
                BaseLanguage = globalConfig.BaseLanguage,
                AzureTranslationKey = globalConfig.AzureTranslationKey,
                TranslationActive = globalConfig.TranslationActive,
                DeepLexActive = globalConfig.DeepLexActive,
                MirriamDictionaryKey = globalConfig.MirriamDictionaryKey,
                MirriamThesaurusKey = globalConfig.MirriamThesaurusKey,

                QualityChange = Array.Empty<string>(),
                QualityChangeValue = Array.Empty<int>(),

                ValidZones = TemplateCache.GetAll<IZoneTemplate>(true),
                ValidLanguages = ConfigDataCache.GetAll<ILanguage>(),

                //GossipConfigDataObject = gossipConfig,
                //GossipActive = gossipConfig.GossipActive,
                //ClientId = gossipConfig.ClientId,
                //ClientSecret = gossipConfig.ClientSecret,
                //ClientName = gossipConfig.ClientName,
                //SuspendMultiplier = gossipConfig.SuspendMultiplier,
                //SuspendMultiplierMaximum = gossipConfig.SuspendMultiplierMaximum,
                //SupportedChannels = gossipConfig.SupportedChannels,
                //SupportedFeatures = gossipConfig.SupportedFeatures
            };

            return View(dashboardModel);
        }

        public ActionResult ModalErrorOrClose(string Message = "")
        {
            return View("~/Views/GameAdmin/ModalErrorOrClose.cshtml", "_chromelessLayout", Message);
        }

        #region Live Threads
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> StopRunningProcessAsync(string processName)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            Processor.ShutdownLoop(processName, 600, "{0} seconds before " + processName + " is shutdown.", 60);

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopRunningProcess[" + processName + "]", authedUser.GameAccount.GlobalIdentityHandle);
            string message = "Cancel signal sent.";

            return RedirectToAction("Index", new { Message = message });
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> StopRunningAllProcessAsync()
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            Processor.ShutdownAll(600, "{0} seconds before TOTAL WORLD SHUTDOWN.", 60);

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopRunningALLPROCESSES", authedUser.GameAccount.GlobalIdentityHandle);
            string message = "Cancel signal sent for entire world.";

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion

        #region Running Data
        [Authorize(Roles = "Admin")]
        public ActionResult BackupWorld(string BackupName = "")
        {
            HotBackup hotBack = new();

            hotBack.WriteLiveBackup(BackupName);
            Templates.WriteFullBackup(BackupName);
            ConfigData.WriteFullBackup(BackupName);

            return RedirectToAction("Index", new { Message = "Backup Started" });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult RestoreWorld()
        {
            HotBackup hotBack = new();

            //TODO: Ensure we suspend EVERYTHING going on (fights, etc), add some sort of announcement globally and delay the entire thing on a timer

            //Write the players out first to maintain their positions
            hotBack.WritePlayers();

            //restore everything
            hotBack.RestoreLiveBackup();

            return RedirectToAction("Index", new { Message = "Restore Started" });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult RestartGossipServer()
        {
            IEnumerable<WebSocket> gossipServers = LiveCache.GetAll<WebSocket>();

            foreach (WebSocket server in gossipServers)
            {
                server.Abort();
            }

            //IGossipConfig gossipConfig = ConfigDataCache.Get<IGossipConfig>(new ConfigDataCacheKey(typeof(IGossipConfig), "GossipSettings", ConfigDataType.GameWorld));
            //Func<Member[]> playerList = () => LiveCache.GetAll<IPlayer>()
            //    .Where(player => player.Descriptor != null && player.Template<IPlayerTemplate>().Account.Config.GossipSubscriber)
            //    .Select(player => new Member()
            //    {
            //        Name = player.AccountHandle,
            //        WriteTo = (message) => player.WriteTo(new string[] { message }),
            //        BlockedMembers = player.Template<IPlayerTemplate>().Account.Config.Acquaintences.Where(acq => !acq.IsFriend).Select(acq => acq.PersonHandle),
            //        Friends = player.Template<IPlayerTemplate>().Account.Config.Acquaintences.Where(acq => acq.IsFriend).Select(acq => acq.PersonHandle)
            //    }).ToArray();

            //void exceptionLogger(Exception ex) => LoggingUtility.LogError(ex);
            //void activityLogger(string message) => LoggingUtility.Log(message, LogChannels.GossipServer);

            //GossipClient gossipServer = new(gossipConfig, exceptionLogger, activityLogger, playerList);

            //Task.Run(() => gossipServer.Launch());

            //LiveCache.Add(gossipServer, "GossipWebClient");

            return RedirectToAction("Index", new { Message = "Gossip Server Restarted" });
        }
        #endregion

        #region "Global Config"
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GlobalConfigAsync(DashboardViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            globalConfig.WebsocketPortalActive = vModel.WebsocketPortalActive;
            globalConfig.AdminsOnly = vModel.AdminsOnly;
            globalConfig.UserCreationActive = vModel.UserCreationActive;
            globalConfig.BaseLanguage = vModel.BaseLanguage;

            globalConfig.AzureTranslationKey = vModel.AzureTranslationKey;
            globalConfig.TranslationActive = vModel.TranslationActive;

            globalConfig.DeepLexActive = vModel.DeepLexActive;
            globalConfig.MirriamDictionaryKey = vModel.MirriamDictionaryKey;
            globalConfig.MirriamThesaurusKey = vModel.MirriamThesaurusKey;

            string message;
            if (globalConfig.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditGlobalConfig[" + globalConfig.UniqueKey.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GossipConfigAsync(DashboardViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            //IGossipConfig gossipConfig = ConfigDataCache.Get<IGossipConfig>(new ConfigDataCacheKey(typeof(IGossipConfig), "GossipSettings", ConfigDataType.GameWorld));

            //gossipConfig.GossipActive = vModel.GossipActive;
            //gossipConfig.ClientId = vModel.ClientId;
            //gossipConfig.ClientName = vModel.ClientName;
            //gossipConfig.ClientSecret = vModel.ClientSecret;
            //gossipConfig.SuspendMultiplierMaximum = vModel.SuspendMultiplierMaximum;
            //gossipConfig.SuspendMultiplier = vModel.SuspendMultiplier;
            //gossipConfig.SupportedChannels = vModel.SupportedChannels;
            //gossipConfig.SupportedFeatures = vModel.SupportedFeatures;

            //string message;
            //if (gossipConfig.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            //{
            //    LoggingUtility.LogAdminCommandUsage("*WEB* - EditGossipConfig[" + gossipConfig.UniqueKey.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            //    message = "Edit Successful.";
            //}
            //else
            //{
            //    message = "Error; Edit failed.";
            //}

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion
    }
}
