﻿using NetMudCore.Authentication;
using NetMudCore.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class MaterialsViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IMaterial> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public MaterialsViewModel(IEnumerable<IMaterial> items)
        {
            Items = items;
        }
    }
}