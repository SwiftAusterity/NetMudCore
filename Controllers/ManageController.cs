﻿using NetMudCore.Authentication;
using NetMudCore.Data.Combat;
using NetMudCore.Data.Player;
using NetMudCore.Data.Players;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Combat;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Players;
using NetMudCore.Models.Admin;
using NetMudCore.Models.PlayerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace NetMudCore.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private SignInManager<ApplicationUser>? _signInManager;
        private UserManager<ApplicationUser>? _userManager;

        public ManageController()
        {
        }

        public ManageController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public SignInManager<ApplicationUser> SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        [HttpGet]
        public async Task<ActionResult> IndexAsync()
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            Account account = user.GameAccount;

            ManageAccountViewModel model = new()
            {
                AuthedUser = user,
                DataObject = account,
                GlobalIdentityHandle = account.GlobalIdentityHandle,
                ComboCount = account.Config.Combos.Count(),
                FightingArtCount = TemplateCache.GetAll<IFightingArt>().Count(art => art.CreatorHandle == user.GlobalIdentityHandle),
                UIModuleCount = TemplateCache.GetAll<IUIModule>(true).Count(uimod => uimod.CreatorHandle.Equals(account.GlobalIdentityHandle)),
                NotificationCount = ConfigDataCache.GetAll<IPlayerMessage>().Count(msg => msg.RecipientAccount == account),
                UITutorialMode = account.Config.UITutorialMode,
                GossipSubscriber = account.Config.GossipSubscriber,
                PermanentlyMuteMusic = account.Config.MusicMuted,
                PermanentlyMuteSound = account.Config.SoundMuted,
                UILanguage = account.Config.UILanguage,
                ChosenRole = ApplicationUser.GetStaffRank(User),
                ValidRoles = (StaffRank[])Enum.GetValues(typeof(StaffRank)),
                ValidLanguages = ConfigDataCache.GetAll<ILanguage>().Where(lang => lang.SuitableForUse && lang.UIOnly)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAccountConfigAsync(ManageAccountViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            Account obj = authedUser.GameAccount;

            obj.Config.UITutorialMode = vModel.UITutorialMode;
            obj.Config.GossipSubscriber = vModel.GossipSubscriber;
            obj.Config.MusicMuted = vModel.PermanentlyMuteMusic;
            obj.Config.SoundMuted = vModel.PermanentlyMuteSound;
            obj.Config.UILanguage = vModel.UILanguage;

            if (vModel.LogChannels != null)
            {
                obj.LogChannelSubscriptions = vModel.LogChannels;
            }

            UserManager.UpdateAsync(authedUser);
            string message;
            if (obj.Config.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.Log("*WEB* - EditGameAccount[" + authedUser.GameAccount.GlobalIdentityHandle + "]", LogChannels.AccountActivity);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        #region Characters
        [HttpGet]
        public ActionResult ManageCharacters(string message)
        {
            ViewBag.StatusMessage = message;

            string userId = User.Identity.GetUserId();
            ManageCharactersViewModel model = new()
            {
                AuthedUser = UserManager.FindById(userId),
                NewCharacter = new PlayerTemplate(),
                ValidGenders = TemplateCache.GetAll<IGender>(),
                ValidRaces = TemplateCache.GetAll<IRace>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCharacter(ManageCharactersViewModel vModel)
        {
            string userId = User.Identity.GetUserId();
            ManageCharactersViewModel model = new()
            {
                AuthedUser = UserManager.FindById(userId),
                ValidGenders = TemplateCache.GetAll<IGender>()
            };

            PlayerTemplate newChar = new()
            {
                Name = vModel.NewCharacter.Name,
                SurName = vModel.NewCharacter.SurName,
                Gender = vModel.NewCharacter.Gender,
                SuperSenses = vModel.NewCharacter.SuperSenses,
                GamePermissionsRank = StaffRank.Player,
                Race = vModel.NewCharacter.Race
            };

            if (User.IsInRole("Admin"))
            {
                newChar.GamePermissionsRank = vModel.NewCharacter.GamePermissionsRank;
            }

            string message = model.AuthedUser.GameAccount.AddCharacter(newChar);

            return RedirectToAction("ManageCharacters", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditCharacter(long id)
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser user = UserManager.FindById(userId);

            IPlayerTemplate obj = PlayerDataCache.Get(new PlayerDataCacheKey(typeof(IPlayerTemplate), user.GlobalIdentityHandle, id));
            AddEditCharacterViewModel model = new()
            {
                AuthedUser  = user,
                DataObject = obj,
                ValidRaces = TemplateCache.GetAll<IRace>(),
                ValidGenders = TemplateCache.GetAll<IGender>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCharacter(long id, AddEditCharacterViewModel vModel)
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);
            IPlayerTemplate obj = PlayerDataCache.Get(new PlayerDataCacheKey(typeof(IPlayerTemplate), authedUser.GlobalIdentityHandle, id));

            obj.Name = vModel.DataObject.Name;
            obj.SurName = vModel.DataObject.SurName;
            obj.Gender = vModel.DataObject.Gender;
            obj.SuperSenses = vModel.DataObject.SuperSenses;
            obj.GamePermissionsRank = vModel.DataObject.GamePermissionsRank;
            obj.Race = vModel.DataObject.Race;
            string message;
            if (obj == null)
            {
                message = "That character does not exist";
            }
            else
            {
                if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.Log("*WEB* - EditCharacter[" + authedUser.GameAccount.GlobalIdentityHandle + "]", LogChannels.AccountActivity);
                    message = "Edit Successful.";
                }
                else
                {
                    message = "Error; edit failed.";
                }
            }

            return RedirectToAction("ManageCharacters", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveCharacter(long removeId, string authorizeRemove)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorizeRemove) || !removeId.ToString().Equals(authorizeRemove))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {

                string userId = User.Identity.GetUserId();
                ManageCharactersViewModel model = new()
                {
                    AuthedUser = UserManager.FindById(userId)
                };

                IPlayerTemplate character = model.AuthedUser.GameAccount.Characters.FirstOrDefault(ch => ch.Id.Equals(removeId));

                if (character == null)
                {
                    message = "That character does not exist";
                }
                else if (character.Remove(model.AuthedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    message = "Character successfully deleted.";
                }
                else
                {
                    message = "Error. Character not removed.";
                }
            }

            return RedirectToAction("ManageCharacters", new { Message = message });
        }
        #endregion

        #region Notifications
        [HttpGet]
        public ActionResult Notifications(string message)
        {
            ViewBag.StatusMessage = message;

            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            IEnumerable<IPlayerMessage> notifications = authedUser.GameAccount.Config.Notifications;

            ManageNotificationsViewModel model = new(notifications)
            {
                AuthedUser = authedUser
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult AddViewNotification(string id)
        {
            string userId = User.Identity.GetUserId();
            AddViewNotificationViewModel model = new()
            {
                AuthedUser = UserManager.FindById(userId)
            };

            if (!string.IsNullOrWhiteSpace(id))
            {
                IPlayerMessage message = ConfigDataCache.Get<IPlayerMessage>(id);

                if (message != null)
                {
                    model.DataObject = message;
                    model.Body = message.Body;
                    model.Recipient = message.RecipientName;
                    model.Subject = message.Subject;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddViewNotification(AddViewNotificationViewModel vModel)
        {
            string message = string.Empty;
            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            try
            {
                if (string.IsNullOrWhiteSpace(vModel.Body) || string.IsNullOrWhiteSpace(vModel.Subject))
                {
                    message = "You must include a valid body and subject.";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(vModel.RecipientAccount))
                    {
                        message = "You must include a valid recipient.";
                    }
                    else
                    {
                        IAccount recipient = Account.GetByHandle(vModel.RecipientAccount);

                        if (recipient == null || recipient.Config.Acquaintences.Any(acq => acq.IsFriend == false && acq.PersonHandle.Equals(authedUser.GameAccount.GlobalIdentityHandle)))
                        {
                            message = "You must include a valid recipient.";
                        }
                        else
                        {
                            PlayerMessage newMessage = new()
                            {
                                Body = vModel.Body,
                                Subject = vModel.Subject,
                                Sender = authedUser.GameAccount,
                                RecipientAccount = recipient
                            };

                            IPlayerTemplate recipientCharacter = TemplateCache.GetByName<IPlayerTemplate>(vModel.Recipient);

                            if (recipientCharacter != null)
                            {
                                newMessage.Recipient = recipientCharacter;
                            }

                            //messages come from players always here
                            if (newMessage.Save(authedUser.GameAccount, StaffRank.Player))
                            {
                                message = "Successfully sent.";
                            }
                            else
                            {
                                LoggingUtility.Log("Message unsuccessful.", LogChannels.SystemWarnings);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
            }

            return RedirectToAction("Notifications", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsReadNotification(string id, AddViewNotificationViewModel vModel)
        {
            string message = string.Empty;
            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    IPlayerMessage notification = ConfigDataCache.Get<IPlayerMessage>(id);

                    if (notification != null)
                    {
                        notification.Read = true;
                        notification.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User));
                    }
                }
                else
                {
                    message = "Invalid message.";
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
            }

            return RedirectToAction("Notifications", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveNotification(string ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {

                string userId = User.Identity.GetUserId();
                ApplicationUser authedUser = UserManager.FindById(userId);

                IPlayerMessage notification = authedUser.GameAccount.Config.Notifications.FirstOrDefault(ch => ch.UniqueKey.Equals(ID));

                if (notification == null)
                {
                    message = "That message does not exist";
                }
                else if (notification.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    message = "Message successfully deleted.";
                }
                else
                {
                    message = "Error. Message not removed.";
                }
            }

            return RedirectToAction("Notifications", new { Message = message });
        }
        #endregion

        #region Acquaintences
        [HttpGet]
        public ActionResult Acquaintences(string message = "")
        {
            ViewBag.StatusMessage = message;

            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            IEnumerable<IAcquaintence> acquaintences = authedUser.GameAccount.Config.Acquaintences;

            ManageAcquaintencesViewModel model = new(acquaintences)
            {
                AuthedUser = authedUser
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAcquaintence(string AcquaintenceName, bool IsFriend, bool GossipSystem, string Notifications)
        {
            string message = string.Empty;
            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            if (AcquaintenceName.Equals(authedUser.GlobalIdentityHandle, StringComparison.InvariantCultureIgnoreCase))
            {
                message = "You can't become an acquaintence of yourself.";
            }
            else
            {
                List<AcquaintenceNotifications> notificationsList = new();

                foreach (string notification in Notifications.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    AcquaintenceNotifications anShort = (AcquaintenceNotifications)Enum.Parse(typeof(AcquaintenceNotifications), notification);

                    notificationsList.Add(anShort);
                }

                Acquaintence newAcq = new()
                {
                    PersonHandle = AcquaintenceName,
                    IsFriend = IsFriend,
                    GossipSystem = GossipSystem,
                    NotificationSubscriptions = notificationsList.ToArray()
                };

                List<IAcquaintence> acquaintences = authedUser.GameAccount.Config.Acquaintences.ToList();

                if (acquaintences.Any(aq => aq.PersonHandle == newAcq.PersonHandle))
                {
                    acquaintences.Remove(newAcq);
                }

                acquaintences.Add(newAcq);
                authedUser.GameAccount.Config.Acquaintences = acquaintences;

                if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    message = "Acquaintence successfully added.";
                }
                else
                {
                    message = "Error. Acquaintence not added.";
                }
            }

            return RedirectToAction("Acquaintences", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Manage/RemoveAcquaintence/{ID?}/{authorize?}")]
        public ActionResult RemoveAcquaintence(string ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {

                string userId = User.Identity.GetUserId();
                ApplicationUser authedUser = UserManager.FindById(userId);

                IAcquaintence acquaintence = authedUser.GameAccount.Config.Acquaintences.FirstOrDefault(ch => ch.PersonHandle.Equals(ID));

                if (acquaintence == null)
                {
                    message = "That Acquaintence does not exist";
                }
                else
                {
                    List<IAcquaintence> acquaintences = authedUser.GameAccount.Config.Acquaintences.ToList();

                    acquaintences.Remove(acquaintence);

                    if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                    {
                        message = "Acquaintence successfully deleted.";
                    }
                    else
                    {
                        message = "Error. Acquaintence not removed.";
                    }
                }
            }

            return RedirectToAction("Acquaintences", new { Message = message });
        }
        #endregion

        #region Fighting Arts
        public async Task<ActionResult> FightingArtsAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            ManageFightingArtViewModel vModel = new(TemplateCache.GetAll<IFightingArt>().Where(art => art.CreatorHandle == authedUser.GlobalIdentityHandle))
            {
                AuthedUser = authedUser,
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/Manage/FightingArts.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Manage/FightingArtRemove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public async Task<ActionResult> FightingArtRemoveAsync(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IFightingArt obj = TemplateCache.Get<IFightingArt>(removeId);

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

                IFightingArt obj = TemplateCache.Get<IFightingArt>(unapproveId);

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
        public async Task<ActionResult> FightingArtAddAsync()
        {
            AddEditFightingArtViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = new FightingArt()
            };

            return View("~/Views/Manage/FightingArtAdd.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FightingArtAddAsync(AddEditFightingArtViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IFightingArt newObj = vModel.DataObject;

            if(newObj.CalculateCostRatio() > 0)
            {
                ViewData.Add("Message", "The Calculated Cost must be equal to or below zero.");
                return View("~/Views/Manage/FightingArtAdd.cshtml", vModel);
            }

            if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddFightingArt[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return RedirectToAction("FightingArts", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> FightingArtEditAsync(int id)
        {
            IFightingArt obj = TemplateCache.Get<IFightingArt>(id);

            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToRoute("ErrorOrClose", new { Message = message });
            }

            AddEditFightingArtViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = obj,
            };

            return View("~/Views/Manage/FightingArtEdit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FightingArtEditAsync(int id, AddEditFightingArtViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IFightingArt obj = TemplateCache.Get<IFightingArt>(id);
            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToRoute("Index", new { StatusMessage = message });
            }

            if (vModel.DataObject.CalculateCostRatio() > 0)
            {
                ViewData.Add("Message", "The Calculated Cost must be equal to or below zero.");
                return View("~/Views/Manage/FightingArtEdit.cshtml", vModel);
            }

            obj.Name = vModel.DataObject.Name;
            obj.ActorCriteria = vModel.DataObject.ActorCriteria;
            obj.Aim = vModel.DataObject.Aim;
            obj.Armor = vModel.DataObject.Armor;
            obj.DistanceChange = vModel.DataObject.DistanceChange;
            obj.DistanceRange = vModel.DataObject.DistanceRange;
            obj.Health = vModel.DataObject.Health;
            obj.HelpText = vModel.DataObject.HelpText;
            obj.Impact = vModel.DataObject.Impact;
            obj.PositionResult = vModel.DataObject.PositionResult;
            obj.Recovery = vModel.DataObject.Recovery;
            obj.RekkaKey = vModel.DataObject.RekkaKey;
            obj.RekkaPosition = vModel.DataObject.RekkaPosition;
            obj.Setup = vModel.DataObject.Setup;
            obj.Stamina = vModel.DataObject.Stamina;
            obj.VictimCriteria = vModel.DataObject.VictimCriteria;
            obj.ResultQuality = vModel.DataObject.ResultQuality;
            obj.AdditiveQuality = vModel.DataObject.AdditiveQuality;
            obj.QualityValue = vModel.DataObject.QualityValue;
            obj.Readiness = vModel.DataObject.Readiness;
            obj.ActionVerb = vModel.DataObject.ActionVerb;
            obj.ActionPredicate = vModel.DataObject.ActionPredicate;


            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditFightingArt[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Combos
        public async Task<ActionResult> CombosAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            ManageCombosViewModel vModel = new(user.GameAccount.Config.Combos)
            {
                AuthedUser = user,
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("Combos", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveComboAsync(string ID, string authorize)
        {
            string message;
            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IFightingArtCombination obj = authedUser.GameAccount.Config.Combos.FirstOrDefault(combo => combo.Name.Equals(ID));

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else
                {
                    authedUser.GameAccount.Config.Combos = authedUser.GameAccount.Config.Combos.Where(combo => !combo.Name.Equals(ID));

                    if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveUIModule[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Delete Successful.";
                    }
                    else
                    {
                        message = "Error; Removal failed.";
                    }
                }
            }

            return RedirectToAction("UIModules", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> AddCombosAsync()
        {
            AddEditCombosViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = new FightingArtCombination()
            };

            return View("AddCombos", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddCombosAsync(AddEditCombosViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            var combos = authedUser.GameAccount.Config.Combos.ToList();

            if(combos.Any(combo => combo.Name.Equals(vModel.DataObject.Name)))
            {
                return RedirectToAction("Combos", new { Message = "Name already taken choose another." });
            }

            combos.Add(vModel.DataObject);

            authedUser.GameAccount.Config.Combos = combos;

            string message;
            if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddCombos[" + vModel.DataObject.Name.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }
            else
            {
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Combos", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditCombosAsync(string id)
        {
            AddEditCombosViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            IFightingArtCombination obj = vModel.AuthedUser.GameAccount.Config.Combos.FirstOrDefault(combo => combo.Name.Equals(id));

            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToAction("Combos", new { Message = message });
            }

            vModel.DataObject = obj;

            return View("EditCombos", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCombosAsync(string id, AddEditCombosViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IFightingArtCombination obj = authedUser.GameAccount.Config.Combos.FirstOrDefault(combo => combo.Name.Equals(id));
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Combos", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Arts = vModel.DataObject.Arts;
            obj.FightingStances = vModel.DataObject.FightingStances;
            obj.IsSystem = vModel.DataObject.IsSystem;
            obj.SituationalUsage = vModel.DataObject.SituationalUsage;

            if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditCombos[" + obj.Name.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Combos", new { Message = message });
        }

        #endregion
        #region UIModules
        public async Task<ActionResult> UIModulesAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            ManageUIModulesViewModel vModel = new(TemplateCache.GetAll<IUIModule>().Where(uimod => uimod.CreatorHandle.Equals(user.GameAccount.GlobalIdentityHandle)))
            {
                AuthedUser = user,
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("UIModules", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveUIModuleAsync(long ID, string authorize)
        {
            string message;
            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                IUIModule obj = TemplateCache.Get<IUIModule>(ID);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveUIModule[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                {
                    message = "Error; Removal failed.";
                }
            }

            return RedirectToAction("UIModules", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> AddUIModuleAsync()
        {
            AddEditUIModuleViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("AddUIModule", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUIModuleAsync(AddEditUIModuleViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            UIModule newObj = new()
            {
                Name = vModel.Name,
                BodyHtml = vModel.BodyHtml,
                Height = vModel.Height,
                Width = vModel.Width,
                HelpText = vModel.HelpText
            };
            string message;
            if (newObj.Create(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddUIModule[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("UIModules", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditUIModuleAsync(long id)
        {
            AddEditUIModuleViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            IUIModule obj = TemplateCache.Get<IUIModule>(id);

            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToAction("UIModules", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.BodyHtml = obj.BodyHtml;
            vModel.Height = obj.Height;
            vModel.Width = obj.Width;
            vModel.HelpText = obj.HelpText;

            return View("EditUIModule", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUIModuleAsync(long id, AddEditUIModuleViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IUIModule obj = TemplateCache.Get<IUIModule>(id);
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("UIModules", new { Message = message });
            }

            obj.Name = vModel.Name;
            obj.BodyHtml = vModel.BodyHtml;
            obj.Height = vModel.Height;
            obj.Width = vModel.Width;
            obj.HelpText = vModel.HelpText;

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditUIModule[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("UIModules", new { Message = message });
        }

        #endregion

        #region Playlists
        [HttpGet]
        public ActionResult Playlists(string message)
        {
            ViewBag.StatusMessage = message;

            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            HashSet<IPlaylist> lists = authedUser.GameAccount.Config.Playlists;

            ManagePlaylistsViewModel model = new(lists)
            {
                AuthedUser = authedUser
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> AddPlaylistAsync()
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            IPlayerTemplate currentCharacter = authedUser.GameAccount.Characters.FirstOrDefault(chr => chr.Id == authedUser.GameAccount.CurrentlySelectedCharacter);

            AddEditPlaylistViewModel vModel = new()
            {
                AuthedUser = authedUser,
                ValidSongs = ContentUtility.GetMusicTracksForZone(currentCharacter?.CurrentLocation?.CurrentZone)
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPlaylistAsync(AddEditPlaylistViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            HashSet<IPlaylist> existingPlaylists = authedUser.GameAccount.Config.Playlists;

            if (existingPlaylists.Any(list => list.Name.Equals(vModel.Name)))
                message = "A playlist by that name already exists.";
            else if (vModel.SongList == null || vModel.SongList.Length == 0)
                message = "Your playlist needs at least one song in it.";
            else
            {
                Playlist playlist = new()
                {
                    Name = vModel.Name,
                    Songs = new HashSet<string>(vModel.SongList)
                };

                authedUser.GameAccount.Config.Playlists.Add(playlist);

                if (!authedUser.GameAccount.Config.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.Log("*WEB* - AddPlaylist[" + vModel.Name + "]", LogChannels.AccountActivity);
                }
            }

            return RedirectToAction("Playlists", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditPlaylistAsync(string name)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            HashSet<IPlaylist> existingPlaylists = authedUser.GameAccount.Config.Playlists;
            IPlaylist obj = existingPlaylists.FirstOrDefault(list => list.Name.Equals(name));

            if (obj == null)
            {
                return RedirectToAction("Playlists", new { Message = "That playlist does not exist." });
            }

            IPlayerTemplate currentCharacter = authedUser.GameAccount.Characters.FirstOrDefault(chr => chr.Id == authedUser.GameAccount.CurrentlySelectedCharacter);
            AddEditPlaylistViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                Name = obj.Name,
                DataObject = obj,
                ValidSongs = ContentUtility.GetMusicTracksForZone(currentCharacter?.CurrentLocation?.CurrentZone)
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPlaylistAsync(AddEditPlaylistViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            HashSet<IPlaylist> existingPlaylists = authedUser.GameAccount.Config.Playlists;
            IPlaylist obj = existingPlaylists.FirstOrDefault(list => list.Name.Equals(vModel.Name));

            if (obj == null)
            {
                return RedirectToAction("Playlists", new { Message = "That playlist does not exist." });
            }

            authedUser.GameAccount.Config.Playlists.Remove(obj);

            Playlist playlist = new()
            {
                Name = vModel.Name,
                Songs = new HashSet<string>(vModel.SongList)
            };

            authedUser.GameAccount.Config.Playlists.Add(playlist);

            if (!authedUser.GameAccount.Config.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                message = "Error; Edit failed.";
            else
            {
                LoggingUtility.Log("*WEB* - EditPlaylist[" + vModel.Name + "]", LogChannels.AccountActivity);
            }

            return RedirectToAction("Playlists", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Manage/RemovePlaylist/{ID?}/{authorize?}")]
        public async Task<ActionResult> RemovePlaylistAsync(string removePlaylistName = "", string authorizeRemovePlaylist = "")
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorizeRemovePlaylist))
                message = "You must check the proper authorize radio button first.";
            else
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                string[] values = authorizeRemovePlaylist.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Length != 2)
                    message = "You must check the proper authorize radio button first.";
                else
                {
                    IPlaylist origin = authedUser.GameAccount.Config.Playlists.FirstOrDefault(list => list.Name.Equals(removePlaylistName));

                    if (origin == null)
                        message = "That playlist does not exist";
                    else
                    {
                        authedUser.GameAccount.Config.Playlists = new HashSet<IPlaylist>(authedUser.GameAccount.Config.Playlists.Where(list => !list.Name.Equals(removePlaylistName)));

                        if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                        {
                            LoggingUtility.Log("*WEB* - RemoveZonePath[" + removePlaylistName + "]", LogChannels.AccountActivity);
                            message = "Delete Successful.";
                        }
                        else
                            message = "Error; Removal failed.";
                    }
                }
            }

            return RedirectToAction("Playlists", new { Message = message });
        }
        #endregion

        #region AuthStuff
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false);
                }
                return RedirectToAction("Index");
            }
            AddErrors(result);
            return View(model);
        }

        [HttpGet]
        public ActionResult SetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false);
                    }
                    return RedirectToAction("Index");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Helpers
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", String.Format("{0} ({1})", error.Description, error.Code));
            }
        }
        #endregion
    }
}