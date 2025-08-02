using NetMudCore.Authentication;
using NetMudCore.DataStructure.Administrative;

namespace NetMudCore.Models
{
    public class BlogViewModel(IEnumerable<IJournalEntry> items) : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }
        public IEnumerable<IJournalEntry> Items { get; set; } = items;
        public IEnumerable<Tuple<string, int>> MonthYearPairs { get; set; }
        public string[] IncludeTags { get; set; } = Array.Empty<string>();
        public string[] AllTags { get; set; } = Array.Empty<string>();
    }
}