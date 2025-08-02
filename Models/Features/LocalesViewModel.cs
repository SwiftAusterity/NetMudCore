using NetMudCore.Authentication;
using NetMudCore.DataStructure.Locale;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class LocalesViewModel(IEnumerable<ILocaleTemplate> items) : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<ILocaleTemplate> Items { get; set; } = items;

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }
    }
}