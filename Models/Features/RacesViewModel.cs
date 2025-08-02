using NetMudCore.Authentication;
using NetMudCore.DataStructure.Architectural.ActorBase;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class RacesViewModel(IEnumerable<IRace> items) : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IRace> Items { get; set; } = items;

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }
    }
}