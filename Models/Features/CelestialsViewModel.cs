using NetMudCore.Authentication;
using NetMudCore.DataStructure.Gaia;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class CelestialsViewModel(IEnumerable<ICelestial> items) : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<ICelestial> Items { get; set; } = items;

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }
    }
}