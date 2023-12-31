﻿using NetMudCore.Authentication;
using NetMudCore.DataStructure.NaturalResource;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Features
{
    public class MineralsViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public IEnumerable<IMineral> Items { get; set; }

        [Display(Name = "Search Term", Description = "Filter by the name of the item.")]
        [DataType(DataType.Text)]
        public string SearchTerm { get; set; }

        public MineralsViewModel(IEnumerable<IMineral> items)
        {
            Items = items;
        }
    }
}