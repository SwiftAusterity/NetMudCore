using NetMudCore.Authentication;
using NetMudCore.Communication.Lexical;
using NetMudCore.Data.Locale;
using NetMudCore.Data.Room;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Locale;
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
    public class RoomController : Controller
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

        public RoomController()
        {
        }

        public RoomController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageRoomTemplateViewModel vModel = new(TemplateCache.GetAll<IRoomTemplate>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Room/Index.cshtml", vModel);
        }

        [HttpGet]
        public ActionResult Map(long ID)
        {
            RoomMapViewModel vModel = new()
            {
                Here = TemplateCache.Get<IRoomTemplate>(ID)
            };

            return View("~/Views/GameAdmin/Room/Map.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Room/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public async Task<ActionResult> RemoveAsync(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveRoom[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveRoom[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
        public async Task<ActionResult> AddAsync(long localeId)
        {
            ILocaleTemplate myLocale = TemplateCache.Get<ILocaleTemplate>(localeId);

            if (myLocale == null)
            {
                return RedirectToAction("Index", new { Message = "Invalid Locale" });
            }

            AddEditRoomTemplateViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidModels = TemplateCache.GetAll<IDimensionalModelData>(),
                ValidZones = TemplateCache.GetAll<IZoneTemplate>(),
                ValidRooms = TemplateCache.GetAll<IRoomTemplate>(),
                ValidLocales = TemplateCache.GetAll<ILocaleTemplate>().Where(locale => locale.Id != localeId),
                ZonePathway = new PathwayTemplate() { Destination = myLocale.ParentLocation },
                LocaleRoomPathway = new PathwayTemplate(),
                LocaleRoomPathwayDestinationLocale = new LocaleTemplate(),
                DataObject = new RoomTemplate() { ParentLocation = myLocale }
            };

            return View("~/Views/GameAdmin/Room/Add.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAsync(long localeId, AddEditRoomTemplateViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            ILocaleTemplate locale = TemplateCache.Get<ILocaleTemplate>(localeId);

            IRoomTemplate newObj = vModel.DataObject;
            newObj.ParentLocation = locale;
            newObj.Coordinates = new Coordinate(0, 0, 0); //TODO: fix this

            IPathwayTemplate zoneDestination = null;
            if (vModel.ZonePathway?.Destination != null && !string.IsNullOrWhiteSpace(vModel.ZonePathway.Name))
            {
                IZoneTemplate destination = TemplateCache.Get<IZoneTemplate>(vModel.ZonePathway.Destination.Id);
                zoneDestination = new PathwayTemplate()
                {
                    DegreesFromNorth = -1,
                    Name = vModel.ZonePathway.Name,
                    Origin = newObj,
                    Destination = destination,
                    InclineGrade = vModel.ZonePathway.InclineGrade,
                    Model = vModel.ZonePathway.Model
                };
            }

            IPathwayTemplate localeRoomPathway = null;
            if (vModel.LocaleRoomPathwayDestination != null && !string.IsNullOrWhiteSpace(vModel.LocaleRoomPathwayDestination.Name))
            {
                IRoomTemplate destination = TemplateCache.Get<IRoomTemplate>(vModel.LocaleRoomPathwayDestination.Id);
                localeRoomPathway = new PathwayTemplate()
                {
                    DegreesFromNorth = vModel.LocaleRoomPathway.DegreesFromNorth,
                    Name = vModel.LocaleRoomPathway.Name,
                    Origin = newObj,
                    Destination = destination,
                    InclineGrade = vModel.LocaleRoomPathway.InclineGrade,
                    Model = vModel.LocaleRoomPathway.Model
                };
            }


            if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
            {
                zoneDestination?.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User));

                localeRoomPathway?.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User));

                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddRoomTemplate[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditAsync(int id)
        {
            string message = string.Empty;
            IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ErrorOrClose", new { Message = message });
            }

            AddEditRoomTemplateViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidZones = TemplateCache.GetAll<IZoneTemplate>(),
                ValidLocales = TemplateCache.GetAll<ILocaleTemplate>().Where(locale => locale.Id != obj.ParentLocation.Id),
                ValidLocaleRooms = TemplateCache.GetAll<IRoomTemplate>().Where(room => room.Id != obj.Id && room.ParentLocation.Id == obj.ParentLocation.Id),
                ValidRooms = TemplateCache.GetAll<IRoomTemplate>().Where(room => room.Id != obj.Id),
                ValidModels = TemplateCache.GetAll<IDimensionalModelData>(),
                DataObject = obj,
            };

            IPathwayTemplate zoneDestination = obj.GetZonePathways().FirstOrDefault();
            if (zoneDestination != null)
            {
                vModel.ZonePathway = zoneDestination;
            }
            else
            {
                vModel.ZonePathway = new PathwayTemplate() { Destination = obj.ParentLocation.ParentLocation, Origin = obj };
            }

            IPathwayTemplate localeRoomPathway = obj.GetLocalePathways().FirstOrDefault();
            if (localeRoomPathway != null)
            {
                vModel.LocaleRoomPathway = localeRoomPathway;
                vModel.LocaleRoomPathwayDestinationLocale = ((IRoomTemplate)localeRoomPathway.Destination).ParentLocation;
                vModel.ValidLocaleRooms = TemplateCache.GetAll<IRoomTemplate>().Where(room => localeRoomPathway.Id == room.ParentLocation.Id);
            }
            else
            {
                vModel.LocaleRoomPathway = new PathwayTemplate() { Origin = obj };
                vModel.LocaleRoomPathwayDestinationLocale = new LocaleTemplate();
            }


            return View("~/Views/GameAdmin/Room/Edit.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(int id, AddEditRoomTemplateViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            IPathwayTemplate zoneDestination = null;
            IPathwayTemplate localeRoomPathway = null;

            IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(id);
            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Medium = vModel.DataObject.Medium;
            obj.Qualities = vModel.DataObject.Qualities;

            if (vModel.ZonePathway?.Destination != null && !string.IsNullOrWhiteSpace(vModel.ZonePathway.Name))
            {
                IZoneTemplate destination = TemplateCache.Get<IZoneTemplate>(vModel.ZonePathway.Destination.Id);
                zoneDestination = obj.GetZonePathways().FirstOrDefault();

                if (zoneDestination == null)
                {
                    zoneDestination = new PathwayTemplate()
                    {
                        DegreesFromNorth = vModel.ZonePathway.DegreesFromNorth,
                        Name = vModel.ZonePathway.Name,
                        Origin = obj,
                        Destination = destination,
                        InclineGrade = vModel.ZonePathway.InclineGrade,
                        Model = vModel.ZonePathway.Model
                    };
                }
                else
                {
                    zoneDestination.Model = vModel.ZonePathway.Model;
                    zoneDestination.Name = vModel.ZonePathway.Name;
                    zoneDestination.InclineGrade = vModel.ZonePathway.InclineGrade;

                    //We switched zones, this makes things more complicated
                    if (zoneDestination.Id != vModel.ZonePathway.Destination.Id)
                    {
                        zoneDestination.Destination = destination;
                    }
                }
            }

            if (vModel.LocaleRoomPathwayDestination != null && !string.IsNullOrWhiteSpace(vModel.LocaleRoomPathwayDestination.Name))
            {
                IRoomTemplate destination = TemplateCache.Get<IRoomTemplate>(vModel.LocaleRoomPathwayDestination.Id);
                localeRoomPathway = obj.GetLocalePathways().FirstOrDefault();

                if (localeRoomPathway == null)
                {
                    localeRoomPathway = new PathwayTemplate()
                    {
                        DegreesFromNorth = vModel.LocaleRoomPathway.DegreesFromNorth,
                        Name = vModel.LocaleRoomPathway.Name,
                        Origin = obj,
                        Destination = destination,
                        InclineGrade = vModel.LocaleRoomPathway.InclineGrade,
                        Model = vModel.LocaleRoomPathway.Model
                    };
                }
                else
                {
                    localeRoomPathway.Model = vModel.LocaleRoomPathway.Model;
                    localeRoomPathway.Name = vModel.LocaleRoomPathway.Name;
                    localeRoomPathway.InclineGrade = vModel.LocaleRoomPathway.InclineGrade;
                    localeRoomPathway.Destination = destination;
                }
            }

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                zoneDestination?.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User));

                localeRoomPathway?.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User));

                LoggingUtility.LogAdminCommandUsage("*WEB* - EditRoomTemplate[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
            }

            return RedirectToRoute("ModalErrorOrClose");
        }

        #region Descriptives
        [HttpGet]
        public async Task<ActionResult> AddEditDescriptiveAsync(long id, short descriptiveType, string phrase)
        {
            string message = string.Empty;

            IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(id);
            if (obj == null)
            {
                message = "That room does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            OccurrenceViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = obj,
                AdminTypeName = "Room"
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

            return View("~/Views/GameAdmin/Room/SensoryEvent.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddEditDescriptiveAsync(long id, OccurrenceViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(id);
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

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - Room AddEditDescriptive[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveDescriptiveAsync(long id, string authorize)
        {
            string message = string.Empty;

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
                    short type = short.Parse(values[0]);
                    string phrase = values[1];

                    IRoomTemplate obj = TemplateCache.Get<IRoomTemplate>(id);

                    if (obj == null)
                    {
                        message = "That does not exist";
                    }
                    else
                    {
                        GrammaticalType grammaticalType = (GrammaticalType)type;
                        ISensoryEvent existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                            && occurrence.Event.Phrase.Equals(phrase, StringComparison.InvariantCultureIgnoreCase));

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

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }
        #endregion
    }
}