﻿using NetMudCore.DataStructure.Architectural.PropertyValidation;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.DataStructure.NPC
{
    [Serializable]
    public class MerchandiseStock
    {
        [Display(Name = "Stock", Description = "The item type to be kept in stock.")]
        [UIHint("Merchandise")]
        [MerchandiseValidator]
        public IMerchandise Item { get; set; }

        [Display(Name = "Rate", Description = "How many of these should be kept in stock. (restock occurs per 15 minutes)")]
        [DataType(DataType.Text)]
        public int Amount { get; set; }

        public MerchandiseStock(IMerchandise item, int amount)
        {
            Item = item;
            Amount = amount;
        }
    }
}
