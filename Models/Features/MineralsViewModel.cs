using NetMudCore.Authentication;
using NetMudCore.DataStructure.NaturalResource;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class MineralsViewModel(IEnumerable<IMineral> items) : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IMineral> Items { get; set; } = items;

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }
    }
}