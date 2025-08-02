using NetMudCore.Authentication;
using NetMudCore.DataStructure.Combat;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class FightingArtsViewModel(IEnumerable<IFightingArt> items) : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IFightingArt> Items { get; set; } = items;

        [Display(Name = "Search Term", Description = "Filter by the name of the fighting art.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }
    }
}