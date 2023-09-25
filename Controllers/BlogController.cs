using NetMudCore.Authentication;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.Models;
using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace NetMudCore.Controllers
{
    public class BlogController : Controller
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

        public BlogController()
        {
        }

        public BlogController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<ActionResult> IndexAsync(string[] includedTags, string monthYearPair = "")
        {
            System.Collections.Generic.IEnumerable<IJournalEntry> validEntries = Enumerable.Empty<IJournalEntry>();
            System.Collections.Generic.IEnumerable<IJournalEntry> filteredEntries = Enumerable.Empty<IJournalEntry>();
            ApplicationUser user = null;

            if(User.Identity.IsAuthenticated)
            {
                user = await UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
                StaffRank userRank = ApplicationUser.GetStaffRank(User);
                validEntries = TemplateCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && (blog.Public || blog.MinimumReadLevel <= userRank));
            }
            else
            {
                validEntries = TemplateCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && blog.Public);
            }

            System.Collections.Generic.IEnumerable<string> allTags = validEntries.SelectMany(blog => blog.Tags).Distinct();
            if (includedTags != null && includedTags.Length > 0)
            {
                validEntries = validEntries.Where(blog => blog.Tags.Any(tag => includedTags.Contains(tag)));
            }

            if (!string.IsNullOrWhiteSpace(monthYearPair))
            {
                string[] pair = monthYearPair.Split("|||", StringSplitOptions.RemoveEmptyEntries);
                string month = pair[0];
                int year = -1;

                if (!string.IsNullOrWhiteSpace(month) && int.TryParse(pair[1], out year))
                {
                    filteredEntries = validEntries.Where(blog =>
                        month.Equals(blog.PublishDate.ToString("MMMM", CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase)
                        && blog.PublishDate.Year.Equals(year));
                }
            }

            if(!filteredEntries.Any())
            {
                filteredEntries = validEntries;
            }

            BlogViewModel vModel = new(filteredEntries.OrderByDescending(obj => obj.PublishDate))
             {
                AuthedUser = user,
                MonthYearPairs = validEntries.Select(blog => new Tuple<string, int>(blog.PublishDate.ToString("MMMM", CultureInfo.InvariantCulture), blog.PublishDate.Year)).Distinct(),
                IncludeTags = includedTags?.Where(tag => tag != "false").ToArray() ?? (Array.Empty<string>()),
                AllTags = allTags.ToArray()
            };

            return View(vModel);
        }
    }
}