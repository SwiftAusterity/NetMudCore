using NetMudCore.Authentication;
using NetMudCore.DataStructure.NaturalResource;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class FloraViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IFlora> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public FloraViewModel(IEnumerable<IFlora> items)
        {
            Items = items;
        }
    }
}