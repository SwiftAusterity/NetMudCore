using NetMudCore.Authentication;
using NetMudCore.DataStructure.Inanimate;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class ItemsViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IInanimateTemplate> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public ItemsViewModel(IEnumerable<IInanimateTemplate> items)
        {
            Items = items;
            SearchTerm = string.Empty;
        }
    }
}