using NetMudCore.Authentication;
using NetMudCore.Communication.Lexical;
using NetMudCore.Data.Inanimate;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class InanimateController : Controller
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

        public InanimateController()
        {
        }

        public InanimateController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageInanimateTemplateViewModel vModel = new(TemplateCache.GetAll<IInanimateTemplate>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Inanimate/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Inanimate/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public async Task<ActionResult> RemoveAsync(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IInanimateTemplate obj = TemplateCache.Get<IInanimateTemplate>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveInanimate[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                IInanimateTemplate obj = TemplateCache.Get<IInanimateTemplate>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, ApplicationUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveInanimate[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            AddEditInanimateTemplateViewModel vModel = new(Template)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
            };

            return View("~/Views/GameAdmin/Inanimate/Add.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAsync(AddEditInanimateTemplateViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IInanimateTemplate newObj = vModel.DataObject;
            string message;
            if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddInanimateTemplate[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditAsync(int id, string ArchivePath = "")
        {
            InanimateTemplate obj = TemplateCache.Get<InanimateTemplate>(id);

            if (obj == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            AddEditInanimateTemplateViewModel vModel = new(ArchivePath, obj)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("~/Views/GameAdmin/Inanimate/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(int id, AddEditInanimateTemplateViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IInanimateTemplate obj = TemplateCache.Get<IInanimateTemplate>(id);
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.AccumulationCap = vModel.DataObject.AccumulationCap;
            obj.Components = vModel.DataObject.Components;
            obj.Descriptives = vModel.DataObject.Descriptives;
            obj.InanimateContainers = vModel.DataObject.InanimateContainers;
            obj.MobileContainers = vModel.DataObject.MobileContainers;
            obj.Model = vModel.DataObject.Model;
            obj.Produces = vModel.DataObject.Produces;
            obj.Qualities = vModel.DataObject.Qualities;
            obj.SkillRequirements = vModel.DataObject.SkillRequirements;

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditInanimateTemplate[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

            IInanimateTemplate obj = TemplateCache.Get<IInanimateTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ModalErrorOrClose", new { Message = message });
            }

            OccurrenceViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = obj,
                AdminTypeName = "Inanimate"
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

            return View("~/Views/GameAdmin/Inanimate/SensoryEvent.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddEditDescriptiveAsync(long id, OccurrenceViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IInanimateTemplate obj = TemplateCache.Get<IInanimateTemplate>(id);
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
                LoggingUtility.LogAdminCommandUsage("*WEB* - Inanimate AddEditDescriptive[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Inanimate/SensoryEvent/Remove/{id?}/{authorize?}", Name = "RemoveInanimateDescriptive")]
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
                    string type = values[0];
                    string phrase = values[1];

                    IInanimateTemplate obj = TemplateCache.Get<IInanimateTemplate>(id);

                    if (obj == null)
                    {
                        message = "That does not exist";
                    }
                    else
                    {
                        GrammaticalType grammaticalType = (GrammaticalType)Enum.Parse(typeof(GrammaticalType), type);
                        ISensoryEvent existingOccurrence = obj.Descriptives.FirstOrDefault(occurrence => occurrence.Event.Role == grammaticalType
                                                                                            && occurrence.Event.Phrase.Equals(phrase, StringComparison.InvariantCultureIgnoreCase));

                        if (existingOccurrence != null)
                        {
                            obj.Descriptives.Remove(existingOccurrence);

                            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                            {
                                LoggingUtility.LogAdminCommandUsage("*WEB* - Inanimate RemoveDescriptive[" + id.ToString() + "|" + type.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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