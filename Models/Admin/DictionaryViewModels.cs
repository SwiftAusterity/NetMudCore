﻿using NetMudCore.Authentication;
using NetMudCore.Data.Linguistic;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Linguistic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMudCore.Models.Admin
{
    public class ManageDictionaryViewModel : PagedDataModel<ILexeme>
    {
        public ManageDictionaryViewModel(IEnumerable<ILexeme> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<ILexeme, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<ILexeme, object> OrderPrimary
        {
            get
            {
                return item => item.Language.Name;
            }
        }


        internal override Func<ILexeme, object> OrderSecondary
        {
            get
            {
                return item => item.Name;
            }
        }
    }

    public class AddEditDictionaryViewModel : PagedDataModel<IDictata>
    {
        internal override Func<IDictata, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IDictata, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IDictata, object> OrderSecondary
        {
            get
            {
                return item => item.FormGroup;
            }
        }

        public AddEditDictionaryViewModel(IEnumerable<IDictata> items)
        : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidWords = ConfigDataCache.GetAll<ILexeme>();
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new Lexeme();
        }

        public AddEditDictionaryViewModel() : base(Enumerable.Empty<IDictata>())
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidWords = ConfigDataCache.GetAll<ILexeme>();
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new Lexeme();
        }

        public IEnumerable<ILanguage> ValidLanguages { get; set; }
        public IEnumerable<ILexeme> ValidWords { get; set; }
        public Lexeme DataObject { get; set; }
    }

    public class AddEditDictataViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public AddEditDictataViewModel(ILexeme parent)
        {
            ParentObject = (Lexeme)parent;
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = new Dictata();
            ValidWords = ConfigDataCache.GetAll<ILexeme>().Where(lex => lex.Language == parent.Language && lex != parent).SelectMany(lex => lex.WordForms).OrderBy(form => form.Name);
        }

        public AddEditDictataViewModel(ILexeme parent, IDictata obj)
        {
            ParentObject = (Lexeme)parent;
            ValidLanguages = ConfigDataCache.GetAll<ILanguage>();
            DataObject = (Dictata)obj;
            ValidWords = ConfigDataCache.GetAll<ILexeme>().Where(lex => lex.Language == parent.Language && lex != parent).SelectMany(lex => lex.WordForms).OrderBy(form => form.Name);
        }

        [Display(Name = "Word", Description = "The new word's name/spelling.")]
        [DataType(DataType.Text)]
        public string Word { get; set; }

        [Display(Name = "Is Synonym", Description = "Is this a synonym (true) or an antonym (false) of the current word.")]
        public bool Synonym { get; set; }

        [Display(Name = "Elegance", Description = "The quality Delta against the current word.")]
        [DataType(DataType.Text)]
        public int Elegance { get; set; }

        [Display(Name = "Severity", Description = "The quality Delta against the current word.")]
        [DataType(DataType.Text)]
        public int Severity { get; set; }

        [Display(Name = "Quality", Description = "The quality Delta against the current word.")]
        [DataType(DataType.Text)]
        public int Quality { get; set; }

        public IEnumerable<ILanguage> ValidLanguages { get; set; }
        public IEnumerable<IDictata> ValidWords { get; set; }
        public Lexeme ParentObject { get; set; }
        public Dictata DataObject { get; set; }
    }
}