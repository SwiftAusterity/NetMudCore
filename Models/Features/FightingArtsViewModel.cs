using NetMudCore.Authentication;
using NetMudCore.DataStructure.Combat;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class FightingArtsViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IFightingArt> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the fighting art.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public FightingArtsViewModel(IEnumerable<IFightingArt> items)
        {
            Items = items;
        }
    }
}