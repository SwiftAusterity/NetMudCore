using NetMudCore.Authentication;
using NetMudCore.DataStructure.Zone;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class ZonesViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IZoneTemplate> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public ZonesViewModel(IEnumerable<IZoneTemplate> items)
        {
            Items = items;
        }
    }
}