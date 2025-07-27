using NetMudCore.Authentication;
using NetMudCore.Data.Linguistic;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace NetMudCore.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class DictionaryController : Controller
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

        public DictionaryController()
        {
        }

        public DictionaryController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageDictionaryViewModel vModel = new(ConfigDataCache.GetAll<ILexeme>())
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Dictionary/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Dictionary/Remove/{removeId?}/{authorizeRemove?}")]
        public async Task<ActionResult> RemoveAsync(string removeId = "", string authorizeRemove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                if (authedUser == null)
                {
                    return BadRequest();
                }

                ILexeme obj = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), removeId, ConfigDataType.Dictionary));

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveConstants[" + removeId + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                {
                    message = "Error; Removal failed.";
                }
            }
            else
            {
                message = "You must check the proper remove or unapprove authorization radio button first.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Purge()
        {
            IEnumerable<ILexeme> dictionary = ConfigDataCache.GetAll<ILexeme>();

            foreach (ILexeme dict in dictionary)
            {
                dict.SystemRemove();
            }

            return RedirectToAction("Index", new { Message = "By fire, it is purged." });
        }

        [HttpGet]
        public async Task<ActionResult> AddAsync()
        {
            AddEditDictionaryViewModel vModel = new()
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("~/Views/GameAdmin/Dictionary/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAsync(AddEditDictionaryViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (authedUser == null)
            {
                return BadRequest();
            }

            ILexeme newObj = vModel.DataObject;
            string message;
            if (newObj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddLexeme[" + newObj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }
            else
            {
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditAsync(string id)
        {
            ILexeme obj = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), id, ConfigDataType.Dictionary));

            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            AddEditDictionaryViewModel vModel = new(obj.WordForms)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty),
                DataObject = (Lexeme)obj
            };

            return View("~/Views/GameAdmin/Dictionary/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(string id, AddEditDictionaryViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (authedUser == null)
            {
                return BadRequest();
            }

            ILexeme obj = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), id, ConfigDataType.Dictionary));
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Language = vModel.DataObject.Language;
            obj.Curated = vModel.DataObject.Curated;

            if (obj.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditLexeme[" + obj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        #region dictata
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddRelatedWordAsync(string lexemeId, string id, AddEditDictataViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (authedUser == null)
            {
                return BadRequest();
            }

            ILexeme lex = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), lexemeId, ConfigDataType.Dictionary));
            if (lex == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            IDictata dict = lex.WordForms.FirstOrDefault(form => form.UniqueKey == id);
            if (dict == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            Lexeme relatedLex = new()
            {
                Name = vModel.Word,
                Language = lex.Language
            };

            Dictata relatedWord = new()
            {
                Name = vModel.Word,
                Severity = dict.Severity + vModel.Severity,
                Quality = dict.Quality + vModel.Quality,
                Elegance = dict.Elegance + vModel.Elegance,
                Tense = dict.Tense,
                Language = dict.Language,
                WordType = dict.WordType,
                Feminine = dict.Feminine,
                Possessive = dict.Possessive,
                Plural = dict.Plural,
                Determinant = dict.Determinant,
                Positional = dict.Positional,
                Perspective = dict.Perspective,
                Semantics = dict.Semantics,
                Context = dict.Context,
                Vulgar = dict.Vulgar
            };

            HashSet<IDictata> synonyms = dict.Synonyms;
            synonyms.Add(dict);

            if (vModel.Synonym)
            {
                relatedWord.Synonyms = synonyms;
                relatedWord.Antonyms = dict.Antonyms;
            }
            else
            {
                relatedWord.Synonyms = dict.Antonyms;
                relatedWord.Antonyms = synonyms;
            }

            relatedLex.AddNewForm(relatedWord);

            string message;
            if (relatedLex.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                if (vModel.Synonym)
                {
                    HashSet<IDictata> mySynonyms = dict.Synonyms;
                    mySynonyms.Add(relatedWord);

                    dict.Synonyms = mySynonyms;
                }
                else
                {
                    HashSet<IDictata> antonyms = dict.Antonyms;
                    antonyms.Add(relatedWord);

                    dict.Antonyms = antonyms;
                }

                lex.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User));
                relatedLex.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User));

                LoggingUtility.LogAdminCommandUsage("*WEB* - EditLexeme[" + lex.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Dictionary/RemoveDictata/{removeId?}/{authorizeRemove?}")]
        public async Task<ActionResult> RemoveDictataAsync(string removeId = "", string authorizeRemove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.Equals(authorizeRemove))
            {
                ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                if (authedUser == null)
                {
                    return BadRequest();
                }

                ILexeme lex = ConfigDataCache.Get<ILexeme>(removeId[..(removeId.LastIndexOf("_") - 1)]);
                IDictata? obj = lex?.WordForms?.FirstOrDefault(form => form.UniqueKey == removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else
                {
                    HashSet<IDictata> wordForms = lex.WordForms.ToHashSet();
                    wordForms.RemoveWhere(form => form.UniqueKey == removeId);
                    lex.WordForms = wordForms.ToArray();

                    lex.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User));
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveConstants[" + removeId + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
            }
            else
            {
                message = "You must check the proper remove or unapprove authorization radio button first.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> AddDictataAsync(string lexemeId)
        {
            ILexeme lex = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), lexemeId, ConfigDataType.Dictionary));
            if (lex == null)
            {
                string message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            AddEditDictataViewModel vModel = new(lex)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("~/Views/GameAdmin/Dictionary/AddDictata.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddDictataAsync(string lexemeId, AddEditDictataViewModel vModel)
        {
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (authedUser == null)
            {
                return BadRequest();
            }

            ILexeme lex = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), lexemeId, ConfigDataType.Dictionary));
            if (lex == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            IDictata newObj = vModel.DataObject;

            lex.AddNewForm(newObj);

            string message;
            if (lex.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddDictata[" + newObj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }
            else
            {
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public async Task<ActionResult> EditDictataAsync(string lexemeId, string id)
        {
            ILexeme lex = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), lexemeId, ConfigDataType.Dictionary));
            IDictata obj = lex?.WordForms?.FirstOrDefault(form => form.UniqueKey == id);

            if (obj == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            AddEditDictataViewModel vModel = new(lex, obj)
            {
                AuthedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty)
            };

            return View("~/Views/GameAdmin/Dictionary/EditDictata.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDictataAsync(string lexemeId, string id, AddEditDictataViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser? authedUser = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (authedUser == null)
            {
                return BadRequest();
            }

            ILexeme lex = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), lexemeId, ConfigDataType.Dictionary));
            IDictata obj = lex?.WordForms?.FirstOrDefault(form => form.UniqueKey == id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Severity = vModel.DataObject.Severity;
            obj.Quality = vModel.DataObject.Quality;
            obj.Elegance = vModel.DataObject.Elegance;
            obj.Tense = vModel.DataObject.Tense;
            obj.Synonyms = vModel.DataObject.Synonyms;
            obj.Antonyms = vModel.DataObject.Antonyms;
            obj.Language = vModel.DataObject.Language;
            obj.WordType = vModel.DataObject.WordType;
            obj.Feminine = vModel.DataObject.Feminine;
            obj.Possessive = vModel.DataObject.Possessive;
            obj.Plural = vModel.DataObject.Plural;
            obj.Determinant = vModel.DataObject.Determinant;
            obj.Positional = vModel.DataObject.Positional;
            obj.Perspective = vModel.DataObject.Perspective;
            obj.Semantics = vModel.DataObject.Semantics;
            obj.Vulgar = vModel.DataObject.Vulgar;
            obj.Context = vModel.DataObject.Context;

            lex.AddNewForm(obj);

            if (lex.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User)))
            {
                foreach (IDictata syn in obj.Synonyms)
                {
                    if (!syn.Synonyms.Any(dict => dict == obj))
                    {
                        HashSet<IDictata> synonyms = syn.Synonyms;
                        synonyms.Add(obj);

                        ILexeme synLex = syn.GetLexeme();
                        syn.Synonyms = synonyms;

                        synLex.AddNewForm(syn);
                        synLex.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User));
                    }
                }

                foreach (IDictata ant in obj.Antonyms)
                {
                    if (!ant.Antonyms.Any(dict => dict == obj))
                    {
                        HashSet<IDictata> antonyms = ant.Antonyms;
                        antonyms.Add(obj);

                        ILexeme antLex = ant.GetLexeme();
                        ant.Antonyms = antonyms;
                        antLex.AddNewForm(ant);
                        antLex.Save(authedUser.GameAccount, ApplicationUser.GetStaffRank(User));
                    }
                }

                LoggingUtility.LogAdminCommandUsage("*WEB* - EditDictata[" + obj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        #endregion
    }
}