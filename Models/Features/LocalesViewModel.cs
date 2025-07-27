using NetMudCore.Authentication;
using NetMudCore.DataStructure.Locale;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class LocalesViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<ILocaleTemplate> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public LocalesViewModel(IEnumerable<ILocaleTemplate> items)
        {
            Items = items;
        }
    }
}