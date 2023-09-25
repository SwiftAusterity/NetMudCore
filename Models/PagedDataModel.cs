﻿using NetMudCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models
{
    public abstract class PagedDataModel<T> : IPagedDataModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public PagedDataModel(IEnumerable<T> items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            Items = items;
            SearchTerms = string.Empty;
            ModelEntityType = typeof(T).ToString();
        }

        public string ModelEntityType { get; set; }

        [Range(1, 10000, ErrorMessage = "Page number must be at least 1.")]
        [RegularExpression("[0-9]+")]
        [Display(Name = "Page Number", Description = "Your current page number.")]
        public int CurrentPageNumber { get; set; }

        [Range(1, 100, ErrorMessage = "Items per page must be between {1} and {2}.")]
        [RegularExpression("[0-9]+")]
        [Display(Name = "Items Per Page", Description = "How many items to display per page.")]
        public int ItemsPerPage { get; set; }

        [StringLength(2000, ErrorMessage = "Search Terms must be at least {2} characters long.", MinimumLength = 2)]
        [Display(Name = "Search", Description = "Filter by keywords and names.")]
        public string SearchTerms { get; set; }

        [Display(Name = "Item", Description = "Valid items for the display list.")]
        public IEnumerable<T> Items { get; private set; }

        internal abstract Func<T, bool> SearchFilter { get; }
        internal abstract Func<T, object> OrderPrimary { get; }
        internal abstract Func<T, object> OrderSecondary { get; }

        public IEnumerable<T> CurrentPageOfItems 
        {
            get
            {
                if(Items == null || !Items.Any())
                {
                    return Enumerable.Empty<T>();
                }

                int skip = (CurrentPageNumber - 1) * ItemsPerPage;
                int take = Math.Abs(Items.Count() - skip) >= ItemsPerPage ? ItemsPerPage : Math.Abs(Items.Count() - skip);

                IEnumerable<T> filteredItems = Items;
                if (!string.IsNullOrWhiteSpace(SearchTerms))
                {
                    filteredItems = filteredItems.Where(item => SearchFilter(item));
                }

                if (OrderPrimary != null)
                {
                    if (OrderSecondary != null)
                    {
                        filteredItems = filteredItems.OrderBy(OrderPrimary).ThenBy(OrderSecondary);
                    }
                    else
                    {
                        filteredItems = filteredItems.OrderBy(OrderPrimary);
                    }
                }

                return filteredItems.Skip(skip).Take(take);
            }
        }

        public int NumberOfPages
        {
            get
            {
                return (int)Math.Ceiling(Math.Max(1, Items.Count() / (double)ItemsPerPage));
            }
        }
    }
}