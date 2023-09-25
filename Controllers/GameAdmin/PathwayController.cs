using NetMudCore.Authentication;
using NetMudCore.Cartography;
using NetMudCore.Communication.Lexical;
using NetMudCore.Data.Room;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Room;
using NetMudCore.Models.Admin;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class PathwayController : Controller
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

        public PathwayController()
        {
        }

        public PathwayController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Pathway/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public async Task<ActionResult> RemoveAsync(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            AddEditPathwayTemplateViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IPathwayTemplate obj = TemplateCache.Get<IPathwayTemplate>(removeId);

                if (obj == null)
                {
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemovePathway[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
                else
                {
                }
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IPathwayTemplate obj = TemplateCache.Get<IPathwayTemplate>(unapproveId);

                if (obj == null)
                {
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapprovePathway[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
                else
                {
                }
            }
            else
            {
            }

            return View("~/Views/GameAdmin/Pathway/AddEdit.cshtml", vModel);
        }


        [HttpGet]
        public async Task<ActionResult> AddAsync(long id, long originRoomId, long destinationRoomId, int degreesFromNorth = 0, int incline = 0)
        {
            //New room or existing room
            if (destinationRoomId.Equals(-1))
            {
                IRoomTemplate origin = TemplateCache.Get<IRoomTemplate>(originRoomId);

                AddPathwayWithRoomTemplateViewModel vModel = new()
                {
                    AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                    ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                    ValidModels = TemplateCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat),
                    ValidRooms = TemplateCache.GetAll<IRoomTemplate>(),
                    Origin = origin,
                    DataObject = new PathwayTemplate() { DegreesFromNorth = degreesFromNorth, InclineGrade = incline },
                    Destination = new RoomTemplate() { ParentLocation = origin.ParentLocation }
                };

                vModel.Destination.ParentLocation = vModel.Origin.ParentLocation;

                return View("~/Views/GameAdmin/Pathway/AddWithRoom.cshtml", "_chromelessLayout", vModel);
            }
            else
            {
                IRoomTemplate origin = TemplateCache.Get<IRoomTemplate>(originRoomId);
                IRoomTemplate destination = TemplateCache.Get<IRoomTemplate>(destinationRoomId);
                IPathwayTemplate pathwayTemplate = TemplateCache.Get<IPathwayTemplate>(id);

                if(pathwayTemplate == null)
                {
                    pathwayTemplate = new PathwayTemplate() { Origin = origin, Destination = destination, DegreesFromNorth = degreesFromNorth, InclineGrade = incline };
                }

                AddEditPathwayTemplateViewModel vModel = new()
                {
                    AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                    ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                    ValidModels = TemplateCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat),
                    ValidRooms = TemplateCache.GetAll<IRoomTemplate>().Where(rm => !rm.Id.Equals(originRoomId)),
                    DataObject = pathwayTemplate
                };

                return View("~/Views/GameAdmin/Pathway/AddEdit.cshtml", "_chromelessLayout", vModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddWithRoomAsync(AddPathwayWithRoomTemplateViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IRoomTemplate origin = TemplateCache.Get<IRoomTemplate>(vModel.Origin.Id);
            IRoomTemplate newRoom = vModel.Destination;
            newRoom.ParentLocation = origin.ParentLocation;

            string message = string.Empty;

            if (newRoom.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) != null)
            {
                IPathwayTemplate newObj = vModel.DataObject;
                newObj.Destination = newRoom;
                newObj.Origin = origin;

                if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
                {
                    message = "Error; Creation failed.";
                }
                else
                {
                    if (vModel.CreateReciprocalPath)
                    {
                        PathwayTemplate reversePath = new()
                        {
                            Name = newObj.Name,
                            DegreesFromNorth = Utilities.ReverseDirection(newObj.DegreesFromNorth),
                            Origin = newObj.Destination,
                            Destination = newObj.Origin,
                            Model = newObj.Model,
                            InclineGrade = newObj.InclineGrade * -1
                        };

                        if (reversePath.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
                        {
                            message = "Reverse Path creation FAILED. Origin path creation SUCCESS.";
                        }
                    }

                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddPathwayWithRoom[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }
            else
            {
                message = "Error; Creation failed.";
            }
            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAsync(AddEditPathwayTemplateViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IPathwayTemplate newObj = vModel.DataObject;

            if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddPathway[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditAsync(long id, long originRoomId, long destinationRoomId)
        {
            string message = string.Empty;
            AddEditPathwayTemplateViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidModels = TemplateCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat),
                ValidRooms = TemplateCache.GetAll<IRoomTemplate>()
            };

            PathwayTemplate obj = TemplateCache.Get<PathwayTemplate>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", "Room", new { Message = message });
            }

            vModel.DataObject = obj;

            return View("~/Views/GameAdmin/Pathway/AddEdit.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(long id, AddEditPathwayTemplateViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IPathwayTemplate obj = TemplateCache.Get<IPathwayTemplate>(id);
            if (obj == null)
            {
                return View("~/Views/GameAdmin/Pathway/AddEdit.cshtml", vModel);
            }

            obj.Name = vModel.DataObject.Name;
            obj.DegreesFromNorth = vModel.DataObject.DegreesFromNorth;
            obj.InclineGrade = vModel.DataObject.InclineGrade;
            obj.Origin = vModel.DataObject.Origin;
            obj.Destination = vModel.DataObject.Destination;

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditPathwayTemplate[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        #region Descriptives
        [HttpGet]
        public async Task<ActionResult> AddEditDescriptiveAsync(long id, short descriptiveType, string phrase)
        {
            string message = string.Empty;

            IPathwayTemplate obj = TemplateCache.Get<IPathwayTemplate>(id);
            if (obj == null)
            {
                message = "That pathway does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            OccurrenceViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = obj,
                AdminTypeName = "Pathway"
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

            return View("~/Views/GameAdmin/Pathway/SensoryEvent.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddEditDescriptiveAsync(long id, OccurrenceViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IPathwayTemplate obj = TemplateCache.Get<IPathwayTemplate>(id);
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
                LoggingUtility.LogAdminCommandUsage("*WEB* - Pathway AddEditDescriptive[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                    IPathwayTemplate obj = TemplateCache.Get<IPathwayTemplate>(id);

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