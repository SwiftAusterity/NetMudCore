using NetMudCore.Authentication;
using NetMudCore.DataStructure.NPC;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class NPCsViewModel(IEnumerable<INonPlayerCharacterTemplate> items) : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<INonPlayerCharacterTemplate> Items { get; set; } = items;

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; } = string.Empty;
    }
}