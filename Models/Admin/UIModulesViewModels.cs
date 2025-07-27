﻿using NetMudCore.Authentication;
using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.PropertyValidation;
using NetMudCore.DataStructure.Players;
using System.ComponentModel.DataAnnotations;

namespace NetMudCore.Models.Admin
{
    public class ManageUIModulesViewModel : PagedDataModel<IUIModule>
    {
        public ManageUIModulesViewModel(IEnumerable<IUIModule> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IUIModule, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IUIModule, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IUIModule, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditUIModuleViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public AddEditUIModuleViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The identifying name of the UI Module. Will be used to find it in the UI Module Load in the client.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [MarkdownStringLengthValidator(ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType("Markdown")]
        [Display(Name = "Help Text", Description = "Descriptive text shown on the UI Modules list page.")]
        [MarkdownBinder]
        public MarkdownString HelpText { get; set; }

        [StringLength(8000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [Display(Name = "Body Content (HTML)", Description = "HTML, css and javascript which drives your module.")]
        [DataType(DataType.MultilineText)]
        public string BodyHtml { get; set; }

        [Range(100, 1000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Height", Description = "The default height of the window your module will spawn in if put into a popup.")]
        [DataType(DataType.Text)]
        public int Height { get; set; }

        [Range(100, 1000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Width", Description = "The default width of the window your module will spawn in if put into a popup.")]
        [DataType(DataType.Text)]
        public int Width { get; set; }

        [Display(Name = "System Default", Description = "If newly registered accounts get this as a defaulted UI Module or not (and which quadrant it goes in).")]
        [DataType(DataType.Text)]
        public int SystemDefault { get; set; }

        public IUIModule DataObject { get; set; }
    }
}