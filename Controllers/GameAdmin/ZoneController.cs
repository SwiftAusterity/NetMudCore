using NetMudCore.Authentication;
using NetMudCore.Communication.Lexical;
using NetMudCore.Data.Room;
using NetMudCore.Data.Zone;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Gaia;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Locale;
using NetMudCore.DataStructure.NaturalResource;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.Zone;
using NetMudCore.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class ZoneController : Controller
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

        public ZoneController()
        {
        }

        public ZoneController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageZoneTemplateViewModel vModel = new(TemplateCache.GetAll<IZoneTemplate>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Zone/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Zone/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public async Task<ActionResult> RemoveAsync(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IZoneTemplate obj = TemplateCache.Get<IZoneTemplate>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveZone[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                IZoneTemplate obj = TemplateCache.Get<IZoneTemplate>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveZone[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            AddEditZoneTemplateViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                ValidWorlds = TemplateCache.GetAll<IGaiaTemplate>(true),
                DataObject = new ZoneTemplate()
            };

            vModel.FloraNaturalResources = TemplateCache.GetAll<IFlora>(true);
            vModel.FaunaNaturalResources = TemplateCache.GetAll<IFauna>(true);
            vModel.MineralNaturalResources = TemplateCache.GetAll<IMineral>(true);

            return View("~/Views/GameAdmin/Zone/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAsync(AddEditZoneTemplateViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IZoneTemplate newObj = vModel.DataObject;
            string message;
            if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddZone[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditAsync(long id)
        {
            IZoneTemplate obj = TemplateCache.Get<IZoneTemplate>(id);

            if (obj == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            IEnumerable<ILocaleTemplate> locales = TemplateCache.GetAll<ILocaleTemplate>().Where(locale => locale.ParentLocation.Equals(obj));

            AddEditZoneTemplateViewModel vModel = new(locales)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = obj,
                ValidWorlds = TemplateCache.GetAll<IGaiaTemplate>(true)
            };

            vModel.FloraNaturalResources = TemplateCache.GetAll<IFlora>(true);
            vModel.FaunaNaturalResources = TemplateCache.GetAll<IFauna>(true);
            vModel.MineralNaturalResources = TemplateCache.GetAll<IMineral>(true);

            return View("~/Views/GameAdmin/Zone/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(AddEditZoneTemplateViewModel vModel, long id)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IZoneTemplate obj = TemplateCache.Get<IZoneTemplate>(id);
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.BaseElevation = vModel.DataObject.BaseElevation;
            obj.PressureCoefficient = vModel.DataObject.PressureCoefficient;
            obj.TemperatureCoefficient = vModel.DataObject.TemperatureCoefficient;
            obj.Hemisphere = vModel.DataObject.Hemisphere;
            obj.World = vModel.DataObject.World;
            obj.FloraResourceSpawn = vModel.DataObject.FloraResourceSpawn;
            obj.FaunaResourceSpawn = vModel.DataObject.FaunaResourceSpawn;
            obj.MineralResourceSpawn = vModel.DataObject.MineralResourceSpawn;

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditZone[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> AddEditLocalePathAsync(long id, long localeId)
        {
            ILocaleTemplate locale = TemplateCache.Get<ILocaleTemplate>(localeId);

            if (locale == null)
            {
                return RedirectToAction("Edit", new { Message = "Locale is invalid.", id });
            }

            IEnumerable<IRoomTemplate> validRooms = TemplateCache.GetAll<IRoomTemplate>().Where(rm => rm.ParentLocation.Equals(locale));

            if (!validRooms.Any())
            {
                return RedirectToAction("Edit", new { Message = "Locale has no rooms.", id });
            }

            IZoneTemplate origin = TemplateCache.Get<IZoneTemplate>(id);

            IPathwayTemplate existingPathway = origin.GetLocalePathways().FirstOrDefault(path => ((IRoomTemplate)path.Destination).ParentLocation.Equals(locale));

            AddEditZonePathwayTemplateViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidModels = TemplateCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat),
                ValidRooms = validRooms,
            };

            if (existingPathway != null)
            {
                vModel.DataObject = existingPathway;
                vModel.DestinationRoom = (IRoomTemplate)existingPathway.Destination;
            }
            else
            {
                vModel.DataObject = new PathwayTemplate() { Origin = origin };
            }

            return View("~/Views/GameAdmin/Zone/AddEditLocalePath.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddLocalePathwayAsync(long id, AddEditZonePathwayTemplateViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IZoneTemplate obj = TemplateCache.Get<IZoneTemplate>(id);
            IPathwayTemplate newObj = vModel.DataObject;

            newObj.Destination = TemplateCache.Get<IRoomTemplate>(vModel.DestinationRoom.Id);
            newObj.Origin = obj;

            if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddPathway[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditLocalePathwayAsync(long id, AddEditZonePathwayTemplateViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IPathwayTemplate obj = TemplateCache.Get<IPathwayTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Edit", new { Message = message, id });
            }

            obj.Name = vModel.DataObject.Name;
            obj.DegreesFromNorth = vModel.DataObject.DegreesFromNorth;
            obj.InclineGrade = vModel.DataObject.InclineGrade;
            obj.Destination = TemplateCache.Get<IRoomTemplate>(vModel.DestinationRoom.Id);

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditPathwayTemplate[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Edit", new { Message = message, obj.Origin.Id });
        }

        #region Descriptives
        [HttpGet]
        public async Task<ActionResult> AddEditDescriptiveAsync(long id, short descriptiveType, string phrase)
        {
            string message = string.Empty;

            IZoneTemplate obj = TemplateCache.Get<IZoneTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            OccurrenceViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = obj,
                AdminTypeName = "Zone"
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
                    Event = new Linguistic.Lexica()
                };
            }

            return View("~/Views/GameAdmin/Zone/SensoryEvent.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddEditDescriptiveAsync(long id, OccurrenceViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IZoneTemplate obj = TemplateCache.Get<IZoneTemplate>(id);
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
                    Event = new Linguistic.Lexica(vModel.SensoryEventDataObject.Event.Type,
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
                existingOccurrence.Event = new Linguistic.Lexica(vModel.SensoryEventDataObject.Event.Type,
                                                        vModel.SensoryEventDataObject.Event.Role,
                                                        vModel.SensoryEventDataObject.Event.Phrase, new LexicalContext(null))
                {
                    Modifiers = vModel.SensoryEventDataObject.Event.Modifiers
                };
            }

            obj.Descriptives.RemoveWhere(occurrence => occurrence.Event.Role == vModel.SensoryEventDataObject.Event.Role
                                                && occurrence.Event.Phrase.Equals(vModel.SensoryEventDataObject.Event.Phrase, StringComparison.InvariantCultureIgnoreCase));

            obj.Descriptives.Add(existingOccurrence);

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - Zone AddEditDescriptive[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Zone/SensoryEvent/Remove/{id?}/{authorize?}", Name = "RemoveZoneDescriptive")]
        public async Task<ActionResult> RemoveDescriptiveAsync(long id = -1, string authorize = "")
        {
            string message = string.Empty;
            long zoneId = -1;

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

                    IZoneTemplate obj = TemplateCache.Get<IZoneTemplate>(id);

                    if (obj == null)
                    {
                        message = "That does not exist";
                    }
                    else
                    {
                        GrammaticalType grammaticalType = (GrammaticalType)Enum.Parse(typeof(GrammaticalType), type);
                        ISensoryEvent existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                            && occurrence.Event.Phrase.Equals(phrase, StringComparison.InvariantCultureIgnoreCase));
                        zoneId = obj.Id;

                        if (existingOccurrence != null)
                        {
                            obj.Descriptives.Remove(existingOccurrence);

                            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                            {
                                LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveDescriptive[" + id.ToString() + "|" + type.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

            return RedirectToAction("Edit", new { Message = message, id = zoneId });
        }
        #endregion
    }
}