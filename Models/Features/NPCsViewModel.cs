using NetMudCore.Authentication;
using NetMudCore.DataStructure.NPC;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class NPCsViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<INonPlayerCharacterTemplate> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public NPCsViewModel(IEnumerable<INonPlayerCharacterTemplate> items)
        {
            Items = items;
            SearchTerm = string.Empty;
        }
    }
}