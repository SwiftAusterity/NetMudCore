using NetMudCore.Authentication;
using NetMudCore.DataStructure.Gaia;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class CelestialsViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<ICelestial> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public CelestialsViewModel(IEnumerable<ICelestial> items)
        {
            Items = items;
        }
    }
}