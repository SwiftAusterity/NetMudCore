using NetMudCore.Authentication;
using NetMudCore.DataStructure.Linguistic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class LanguagesViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<ILanguage> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public LanguagesViewModel(IEnumerable<ILanguage> items)
        {
            Items = items;
        }
    }
}