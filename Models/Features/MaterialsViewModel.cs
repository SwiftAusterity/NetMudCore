using NetMudCore.Authentication;
using NetMudCore.DataStructure.Architectural.EntityBase;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class MaterialsViewModel(IEnumerable<IMaterial> items) : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IMaterial> Items { get; set; } = items;

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }
    }
}