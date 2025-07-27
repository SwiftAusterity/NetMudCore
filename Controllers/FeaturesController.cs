using NetMudCore.Authentication;
using NetMudCore.Commands.Attributes;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Gaia;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Locale;
using NetMudCore.DataStructure.NaturalResource;
using NetMudCore.DataStructure.Combat;
using NetMudCore.DataStructure.NPC;
using NetMudCore.DataStructure.System;
using NetMudCore.DataStructure.Zone;
using NetMudCore.Models.Features;
using NetMudCore.Utility;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace NetMudCore.Controllers
{
    public class FeaturesController : Controller
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

        public FeaturesController()
        {
        }

        public FeaturesController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        #region Template Data
        public async Task<ActionResult> NPCsAsync(string SearchTerm = "")
        {
            try
            {
                IEnumerable<INonPlayerCharacterTemplate> validEntries = TemplateCache.GetAll<INonPlayerCharacterTemplate>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                NPCsViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public async Task<ActionResult> ItemsAsync(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IInanimateTemplate> validEntries = TemplateCache.GetAll<IInanimateTemplate>(true);
                ApplicationUser? user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                ItemsViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public async Task<ActionResult> FloraAsync(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IFlora> validEntries = TemplateCache.GetAll<IFlora>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                FloraViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public async Task<ActionResult> FaunaAsync(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IFauna> validEntries = TemplateCache.GetAll<IFauna>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                FaunaViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public async Task<ActionResult> MineralsAsync(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IMineral> validEntries = TemplateCache.GetAll<IMineral>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                MineralsViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public async Task<ActionResult> RacesAsync(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IRace> validEntries = TemplateCache.GetAll<IRace>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                RacesViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public async Task<ActionResult> WorldsAsync(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IGaiaTemplate> validEntries = TemplateCache.GetAll<IGaiaTemplate>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                WorldsViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public async Task<ActionResult> ZonesAsync(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IZoneTemplate> validEntries = TemplateCache.GetAll<IZoneTemplate>(true).Where(zone => zone.AlwaysDiscovered);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                ZonesViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public async Task<ActionResult> LocalesAsync(string SearchTerm = "")
        {
            try
            {

                IEnumerable<ILocaleTemplate> validEntries = TemplateCache.GetAll<ILocaleTemplate>(true).Where(zone => zone.AlwaysDiscovered);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                LocalesViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public async Task<ActionResult> CelestialsAsync(string SearchTerm = "")
        {
            try
            {

                IEnumerable<ICelestial> validEntries = TemplateCache.GetAll<ICelestial>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                CelestialsViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public async Task<ActionResult> LanguagesAsync(string SearchTerm = "")
        {
            try
            {

                IEnumerable<ILanguage> validEntries = ConfigDataCache.GetAll<ILanguage>();
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                LanguagesViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public async Task<ActionResult> MaterialsAsync(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IMaterial> validEntries = TemplateCache.GetAll<IMaterial>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                }

                MaterialsViewModel vModel = new(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    AuthedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }
        #endregion

        public async Task<ActionResult> HelpAsync(string SearchTerm = "", bool IncludeInGame = true)
        {
            List<IHelp> validEntries = TemplateCache.GetAll<IHelp>(true).ToList();
            ApplicationUser user = null;
            string searcher = SearchTerm.Trim().ToLower();

            if (User.Identity.IsAuthenticated)
            {
                user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                StaffRank userRank = ApplicationUser.GetStaffRank(User);
            }

            if (IncludeInGame)
            {
                //All the entities with helps
                IEnumerable<ILookupData> entityHelps = TemplateCache.GetAll<ILookupData>(true).Where(data => !data.ImplementsType<IHelp>());
                validEntries.AddRange(entityHelps.Select(helpful => new Data.Administrative.Help() { Name = helpful.Name, HelpText = helpful.HelpText }));

                //All the commands
                Assembly commandsAssembly = Assembly.GetAssembly(typeof(CommandParameterAttribute));
                IEnumerable<Type> validTargetTypes = commandsAssembly.GetTypes().Where(t => !t.IsAbstract && t.ImplementsType<IHelpful>());

                foreach (Type command in validTargetTypes)
                {
                    IHelpful instance = (IHelpful)Activator.CreateInstance(command);
                    MarkdownString body = instance.HelpText;
                    string subject = command.Name;

                    validEntries.Add(new Data.Administrative.Help() { Name = subject, HelpText = body });
                }
            }

            HelpViewModel vModel = new(validEntries.Where(help => help.HelpText.ToLower().Contains(searcher) || help.Name.ToLower().Contains(searcher)))
            {
                AuthedUser = user,
                SearchTerm = SearchTerm,
                IncludeInGame = IncludeInGame
            };

            return View(vModel);
        }

        public async Task<ActionResult> FightingArtsAsync(string SearchTerm = "")
        {
            List<IFightingArt> validEntries = TemplateCache.GetAll<IFightingArt>(true).ToList();
            ApplicationUser user = null;
            string searcher = SearchTerm.Trim().ToLower();

            if (User.Identity.IsAuthenticated)
            {
                user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                StaffRank userRank = ApplicationUser.GetStaffRank(User);
            }

            FightingArtsViewModel vModel = new(validEntries.Where(help => help.Name.ToLower().Contains(searcher)))
            {
                AuthedUser = user,
                SearchTerm = SearchTerm
            };

            return View(vModel);
        }

        #region NonDataViews
        public ActionResult Skills()
        {
            return View();
        }

        public ActionResult Lore()
        {
            return View();
        }

        public ActionResult TheWorld()
        {
            return View();
        }
        #endregion
    }
}