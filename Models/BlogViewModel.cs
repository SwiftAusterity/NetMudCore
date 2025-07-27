using NetMudCore.Authentication;
using NetMudCore.DataStructure.Administrative;

namespace NetMudCore.Models
{
    public class BlogViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }
        public IEnumerable<IJournalEntry> Items { get; set; }
        public IEnumerable<Tuple<string, int>> MonthYearPairs { get; set; }
        public string[] IncludeTags { get; set; }
        public string[] AllTags { get; set; }

        public BlogViewModel(IEnumerable<IJournalEntry> items)
        {
            Items = items;
            IncludeTags = Array.Empty<string>();
            AllTags = Array.Empty<string>();
        }
    }
}