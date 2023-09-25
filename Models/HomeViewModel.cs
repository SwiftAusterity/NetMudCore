using NetMudCore.Authentication;
using NetMudCore.DataStructure.Administrative;
using System.Collections.Generic;

namespace NetMudCore.Models
{
    public class HomeViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IJournalEntry LatestPatchNotes { get; set; }
        public IEnumerable<IJournalEntry> LatestNews { get; set; }

        public HomeViewModel()
        {
        }
    }
}