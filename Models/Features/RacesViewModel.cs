using NetMudCore.Authentication;
using NetMudCore.DataStructure.Architectural.ActorBase;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class RacesViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IRace> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public RacesViewModel(IEnumerable<IRace> items)
        {
            Items = items;
        }
    }
}