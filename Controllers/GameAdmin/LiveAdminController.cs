using NetMudCore.Authentication;
using NetMudCore.Communication.Lexical;
using NetMudCore.Data.Zone;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Gaia;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Locale;
using NetMudCore.DataStructure.NPC;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.Zone;
using NetMudCore.Models.Admin;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class LiveAdminController : Controller
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

        public LiveAdminController()
        {
        }

        public LiveAdminController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        #region Zone
        [HttpGet]
        public async Task<ActionResult> ZonesAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveZonesViewModel vModel = new(LiveCache.GetAll<IZone>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/Zone")]
        public async Task<ActionResult> ZoneAsync(string birthMark, ViewZoneViewModel viewModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            ViewZoneViewModel vModel = new(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"LiveAdmin/EditZone")]
        public async Task<ActionResult> EditZoneAsync(string birthMark, ViewZoneViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IZone obj = LiveCache.Get<IZone>(new LiveCacheKey(typeof(Zone), birthMark));
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.BaseElevation = vModel.DataObject.BaseElevation;
            obj.Hemisphere = vModel.DataObject.Hemisphere;
            obj.Humidity = vModel.DataObject.Humidity;
            obj.Temperature = vModel.DataObject.Temperature;

            //obj.NaturalResources = vModel.DataObject.NaturalResources;
            obj.Qualities = vModel.DataObject.Qualities;

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - LIVE DATA - EditZone[" + obj.BirthMark + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Zone", new { Message = message, birthMark });
        }

        [HttpGet]
        [Route(@"LiveAdmin/Zone/AddEditDescriptive")]
        public async Task<ActionResult> AddEditZoneDescriptiveAsync(string birthMark, short descriptiveType, string phrase)
        {
            string message = string.Empty;

            IZone obj = LiveCache.Get<IZone>(new LiveCacheKey(typeof(Zone), birthMark));
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            LiveOccurrenceViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = obj,
                AdminTypeName = "LiveAdmin/Zone"
            };

            if (descriptiveType > -1)
            {
                GrammaticalType grammaticalType = (GrammaticalType)descriptiveType;
                vModel.SensoryEventDataObject = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                        && occurrence.Event.Phrase.Equals(phrase, StringComparison.InvariantCultureIgnoreCase));
            }

            if (vModel.SensoryEventDataObject != null)
            {
                vModel.LexicaDataObject = vModel.SensoryEventDataObject.Event;
            }
            else
            {
                vModel.SensoryEventDataObject = new SensoryEvent
                {
                    Event = new Data.Linguistic.Lexica()
                };
            }

            return View("~/Views/LiveAdmin/Zone/SensoryEvent.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"LiveAdmin/Zone/AddEditDescriptive")]
        public async Task<ActionResult> AddEditZoneDescriptiveAsync(string birthMark, LiveOccurrenceViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IZone obj = LiveCache.Get<IZone>(new LiveCacheKey(typeof(Zone), birthMark));
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            ISensoryEvent existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == vModel.SensoryEventDataObject.Event.Role
                                                                                && occurrence.Event.Phrase.Equals(vModel.SensoryEventDataObject.Event.Phrase, StringComparison.InvariantCultureIgnoreCase));

            if (existingOccurrence == null)
            {
                existingOccurrence = new SensoryEvent(vModel.SensoryEventDataObject.SensoryType)
                {
                    Strength = vModel.SensoryEventDataObject.Strength,
                    Event = new Data.Linguistic.Lexica(vModel.SensoryEventDataObject.Event.Type,
                                        vModel.SensoryEventDataObject.Event.Role,
                                        vModel.SensoryEventDataObject.Event.Phrase, new LexicalContext(null))
                    {
                        Modifiers = vModel.SensoryEventDataObject.Event.Modifiers
                    }
                };
            }
            else
            {
                existingOccurrence.Strength = vModel.SensoryEventDataObject.Strength;
                existingOccurrence.SensoryType = vModel.SensoryEventDataObject.SensoryType;
                existingOccurrence.Event = new Data.Linguistic.Lexica(vModel.SensoryEventDataObject.Event.Type,
                                                        vModel.SensoryEventDataObject.Event.Role,
                                                        vModel.SensoryEventDataObject.Event.Phrase, new LexicalContext(null))
                {
                    Modifiers = vModel.SensoryEventDataObject.Event.Modifiers
                };
            }

            obj.Descriptives.RemoveWhere(occurrence => occurrence.Event.Role == vModel.SensoryEventDataObject.Event.Role
                                                && occurrence.Event.Phrase.Equals(vModel.SensoryEventDataObject.Event.Phrase, StringComparison.InvariantCultureIgnoreCase));

            obj.Descriptives.Add(existingOccurrence);

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - LIVE DATA - Zone AddEditDescriptive[" + obj.BirthMark + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"LiveAdmin/Zone/SensoryEvent/Remove/{id?}/{authorize?}")]
        public async Task<ActionResult> RemoveZoneDescriptiveAsync(string id = "", string authorize = "")
        {
            string message = string.Empty;
            string zoneId = "";

            if (string.IsNullOrWhiteSpace(authorize))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                string[] values = authorize.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Length != 2)
                {
                    message = "You must check the proper authorize radio button first.";
                }
                else
                {
                    string type = values[0];
                    string phrase = values[1];

                    IZone obj = LiveCache.Get<IZone>(new LiveCacheKey(typeof(IZone), id));

                    if (obj == null)
                    {
                        message = "That does not exist";
                    }
                    else
                    {
                        GrammaticalType grammaticalType = (GrammaticalType)Enum.Parse(typeof(GrammaticalType), type);
                        ISensoryEvent existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                            && occurrence.Event.Phrase.Equals(phrase, StringComparison.InvariantCultureIgnoreCase));
                        zoneId = obj.BirthMark;

                        if (existingOccurrence != null)
                        {
                            obj.Descriptives.Remove(existingOccurrence);

                            if (obj.Save())
                            {
                                LoggingUtility.LogAdminCommandUsage("*WEB* - LIVE DATA - RemoveDescriptive[" + id.ToString() + "|" + type.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                                message = "Delete Successful.";
                            }
                            else
                            {
                                message = "Error; Removal failed.";
                            }
                        }
                        else
                        {
                            message = "That does not exist";
                        }
                    }
                }
            }

            return RedirectToAction("Zone", new { Message = message, birthMark = id });
        }
        #endregion

        #region World
        [HttpGet]
        public async Task<ActionResult> WorldsAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveWorldsViewModel vModel = new(LiveCache.GetAll<IGaia>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/World")]
        public async Task<ActionResult> WorldAsync(string birthMark, ViewZoneViewModel viewModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            ViewGaiaViewModel vModel = new(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"LiveAdmin/EditWorld")]
        public async Task<ActionResult> EditWorldAsync(string birthMark, ViewGaiaViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IGaia obj = LiveCache.Get<IGaia>(new LiveCacheKey(typeof(Gaia), birthMark));
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.RotationalAngle = vModel.DataObject.RotationalAngle;
            obj.OrbitalPosition = vModel.DataObject.OrbitalPosition;
            obj.Macroeconomy = vModel.DataObject.Macroeconomy;
            obj.CelestialPositions = vModel.DataObject.CelestialPositions;
            obj.MeterologicalFronts = vModel.DataObject.MeterologicalFronts;
            obj.CurrentTimeOfDay = vModel.DataObject.CurrentTimeOfDay;

            obj.Qualities = vModel.DataObject.Qualities;

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - LIVE DATA - EditGaia[" + obj.BirthMark + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("World", new { Message = message, birthMark });
        }
        #endregion

        #region items
        [HttpGet]
        public async Task<ActionResult> InanimatesAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveInanimatesViewModel vModel = new(LiveCache.GetAll<IInanimate>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/Inanimate")]
        public async Task<ActionResult> InanimateAsync(string birthMark, ViewZoneViewModel viewModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            ViewInanimateViewModel vModel = new(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }
        #endregion

        #region NPC
        [HttpGet]
        public async Task<ActionResult> NPCsAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveNPCsViewModel vModel = new(LiveCache.GetAll<INonPlayerCharacter>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/NPC")]
        public async Task<ActionResult> NPCAsync(string birthMark, ViewZoneViewModel viewModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            ViewIntelligenceViewModel vModel = new(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }
        #endregion

        #region Room
        [HttpGet]
        public async Task<ActionResult> RoomsAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveRoomsViewModel vModel = new(LiveCache.GetAll<IRoom>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/Room")]
        public async Task<ActionResult> RoomAsync(string birthMark, ViewZoneViewModel viewModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            ViewRoomViewModel vModel = new(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }
        #endregion

        #region Locale
        [HttpGet]
        public async Task<ActionResult> LocalesAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveLocalesViewModel vModel = new(LiveCache.GetAll<ILocale>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/Locale")]
        public async Task<ActionResult> LocaleAsync(string birthMark, ViewZoneViewModel viewModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            ViewLocaleViewModel vModel = new(birthMark)
            {
                AuthedUser = authedUser,
                Elegance = viewModel.Elegance,
                Severity = viewModel.Severity,
                Quality = viewModel.Quality,
                Language = viewModel.Language ?? authedUser.GameAccount.Config.UILanguage
            };

            return View(vModel);
        }
        #endregion
    }
}