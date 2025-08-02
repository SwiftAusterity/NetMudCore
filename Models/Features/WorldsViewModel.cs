using NetMudCore.Authentication;
using NetMudCore.DataStructure.Gaia;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class WorldsViewModel(IEnumerable<IGaiaTemplate> items) : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IGaiaTemplate> Items { get; set; } = items;

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }
    }
}