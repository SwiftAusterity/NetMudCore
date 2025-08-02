using NetMudCore.Authentication;
using NetMudCore.DataStructure.Administrative;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class HelpViewModel(IEnumerable<IHelp> items) : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IHelp> Items { get; set; } = items;

        [Display(Name = "Search Term", Description = "Filter by the subject or body of the help files.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        [Display(Name = "In-Game", Description = "Include help content from in-game entity types and commands.")]
        [UIHint("Boolean")]
        public bool IncludeInGame { get; set; }
    }
}