using NetMudCore.Authentication;
using NetMudCore.Communication.Lexical;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.NPC;
using NetMudCore.Models.Admin;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class NPCController : Controller
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

        public NPCController()
        {
        }

        public NPCController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageNPCDataViewModel vModel = new(TemplateCache.GetAll<INonPlayerCharacterTemplate>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/NPC/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"NPC/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public async Task<ActionResult> RemoveAsync(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                INonPlayerCharacterTemplate obj = TemplateCache.Get<INonPlayerCharacterTemplate>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveNPC[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                INonPlayerCharacterTemplate obj = TemplateCache.Get<INonPlayerCharacterTemplate>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveNPC[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
        public async Task<ActionResult> AddAsync(long Template = -1)
        {
            AddEditNPCDataViewModel vModel = new(Template)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("~/Views/GameAdmin/NPC/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAsync(AddEditNPCDataViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            INonPlayerCharacterTemplate newObj = vModel.DataObject;
            string message;
            if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddNPCData[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditAsync(int id, string ArchivePath = "")
        {
            INonPlayerCharacterTemplate obj = TemplateCache.Get<INonPlayerCharacterTemplate>(id);

            if (obj == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            AddEditNPCDataViewModel vModel = new(ArchivePath, obj)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("~/Views/GameAdmin/NPC/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(int id, AddEditNPCDataViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            INonPlayerCharacterTemplate obj = TemplateCache.Get<INonPlayerCharacterTemplate>(id);
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.SurName = vModel.DataObject.SurName;
            obj.Gender = vModel.DataObject.Gender;
            obj.Race = vModel.DataObject.Race;
            obj.Personality = vModel.DataObject.Personality;
            obj.Qualities = vModel.DataObject.Qualities;
            obj.InventoryRestock = vModel.DataObject.InventoryRestock;
            obj.TeachableProficencies = vModel.DataObject.TeachableProficencies;
            obj.WillPurchase = vModel.DataObject.WillPurchase;
            obj.WillSell = vModel.DataObject.WillSell;

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditNPCData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        #region Descriptives
        [HttpGet]
        public async Task<ActionResult> AddEditDescriptiveAsync(long id, short descriptiveType, string phrase)
        {
            string message = string.Empty;

            INonPlayerCharacterTemplate obj = TemplateCache.Get<INonPlayerCharacterTemplate>(id);
            if (obj == null)
            {
                message = "That zone does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            OccurrenceViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = obj,
                AdminTypeName = "NPC"
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

            return View("~/Views/GameAdmin/NPC/SensoryEvent.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddEditDescriptiveAsync(long id, OccurrenceViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            INonPlayerCharacterTemplate obj = TemplateCache.Get<INonPlayerCharacterTemplate>(id);
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
                LoggingUtility.LogAdminCommandUsage("*WEB* - NonPlayerCharacter AddEditDescriptive[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                    INonPlayerCharacterTemplate obj = TemplateCache.Get<INonPlayerCharacterTemplate>(id);

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