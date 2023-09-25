using NetMudCore.Authentication;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.System;
using NetMudCore.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace NetMudCore.Controllers
{
    public class HomeController : Controller
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

        public HomeController()
        {
        }

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync()
        {
            ApplicationUser user = null;
            HomeViewModel vModel = new();

            try
            {
                IEnumerable<IJournalEntry> validEntries;
                if (User.Identity?.IsAuthenticated ?? false)
                {
                    user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                    StaffRank userRank = ApplicationUser.GetStaffRank(User);
                    validEntries = TemplateCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && (blog.Public || blog.MinimumReadLevel <= userRank));
                }
                else
                {
                    validEntries = TemplateCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && blog.Public);
                }

                vModel.AuthedUser = user;
                vModel.LatestNews = validEntries.Where(blog => !blog.HasTag("Patch Notes")).OrderByDescending(blog => blog.PublishDate).Take(3);
                vModel.LatestPatchNotes = validEntries.OrderByDescending(blog => blog.PublishDate).FirstOrDefault(blog => blog.HasTag("Patch Notes"));
            }
            catch
            {
                vModel.AuthedUser = user;
                vModel.LatestNews = Enumerable.Empty<IJournalEntry>();
                vModel.LatestPatchNotes = null;
            }

            return View(vModel);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }


        [HttpGet]
        public ActionResult ReportBug()
        {
            BugReportModel vModel = new();

            return View("~/Views/Shared/ReportBug.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        public ActionResult ReportBug(string body)
        {
            if (!string.IsNullOrWhiteSpace(body))
            {
                LoggingUtility.Log(body, LogChannels.BugReport, true);
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = "" });
        }

        [HttpGet]
        public ActionResult WordFight()
        {
            WordFightViewModel vModel = new();

            IEnumerable<ILexeme> lexes = ConfigDataCache.GetAll<ILexeme>();

            var words = lexes.Where(word => !word.Curated && word.SuitableForUse && word.WordForms.Length > 0)
                .SelectMany(lex => lex.WordForms).Where(word => word.Synonyms.Count > 0).OrderBy(word => word.TimesRated);

            vModel.WordOne = words.FirstOrDefault();
            vModel.WordTwo = vModel.WordOne.Synonyms.OrderBy(syn => syn.TimesRated).FirstOrDefault();

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WordFight(short wordOneId, string wordOneName, short wordTwoId, string wordTwoName, WordFightViewModel vModel)
        {
            string message = string.Empty;
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            ILexeme lexOne = ConfigDataCache.Get<ILexeme>(string.Format("{0}_{1}_{2}", ConfigDataType.Dictionary, globalConfig.BaseLanguage.Name, wordOneName));
            ILexeme lexTwo = ConfigDataCache.Get<ILexeme>(string.Format("{0}_{1}_{2}", ConfigDataType.Dictionary, globalConfig.BaseLanguage.Name, wordTwoName));

            if (lexOne != null && lexTwo != null)
            {
                IDictata wordOne = lexOne.GetForm(wordOneId);
                IDictata wordTwo = lexTwo.GetForm(wordTwoId);

                if (wordOne != null || wordTwo != null)
                {
                    switch (vModel.Elegance)
                    {
                        case 1:
                            wordOne.Elegance += 1;
                            wordTwo.Elegance -= 1;
                            break;
                        case 2:
                            wordOne.Elegance -= 1;
                            wordTwo.Elegance += 1;
                            break;
                    }

                    switch (vModel.Severity)
                    {
                        case 1:
                            wordOne.Severity += 1;
                            wordTwo.Severity -= 1;
                            break;
                        case 2:
                            wordOne.Severity -= 1;
                            wordTwo.Severity += 1;
                            break;
                    }
                    switch (vModel.Quality)
                    {
                        case 1:
                            wordOne.Quality += 1;
                            wordTwo.Quality -= 1;
                            break;
                        case 2:
                            wordOne.Quality -= 1;
                            wordTwo.Quality += 1;
                            break;
                    }

                    wordOne.TimesRated += 1;
                    wordTwo.TimesRated += 1;

                    lexOne.PersistToCache();
                    lexOne.SystemSave();
                    
                    lexTwo.PersistToCache();
                    lexTwo.SystemSave();
                }
                else
                {
                    message = "Invalid data";
                }
            }
            else
            {
                message = "Invalid data";
            }

            return RedirectToAction("WordFight", new { Message = message });
        }
    }
}