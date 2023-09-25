using System.ComponentModel.DataAnnotations;

namespace NetMudCore.DataStructure.Linguistic
{
    /// <summary>
    /// The base needed word types for a language to function
    /// </summary>
    public class BaseLanguageMembers
    {
        public BaseLanguageMembers(string neutralPronounSecondPersonSingular, string neutralPronounSecondPersonPlural, string neutralPronounSecondPersonPossessive, string neutralPronounFirstPersonSingular, string neutralPronounFirstPersonPossessive, string articleDeterminant, string articleNonDeterminant, string conjunction, string verbExistentialSingular, string verbExistentialPlural, string prepositionFar, string prepositionNear, string prepositionAttached, string prepositionOn, string prepositionInside, string prepositionAround, string prepositionOf)
        {
            NeutralPronounSecondPersonSingular = neutralPronounSecondPersonSingular;
            NeutralPronounSecondPersonPlural = neutralPronounSecondPersonPlural;
            NeutralPronounSecondPersonPossessive = neutralPronounSecondPersonPossessive;
            NeutralPronounFirstPersonSingular = neutralPronounFirstPersonSingular;
            NeutralPronounFirstPersonPossessive = neutralPronounFirstPersonPossessive;
            ArticleDeterminant = articleDeterminant;
            ArticleNonDeterminant = articleNonDeterminant;
            Conjunction = conjunction;
            VerbExistentialSingular = verbExistentialSingular;
            VerbExistentialPlural = verbExistentialPlural;
            PrepositionFar = prepositionFar;
            PrepositionNear = prepositionNear;
            PrepositionAttached = prepositionAttached;
            PrepositionOn = prepositionOn;
            PrepositionInside = prepositionInside;
            PrepositionAround = prepositionAround;
            PrepositionOf = prepositionOf;
        }

        public BaseLanguageMembers()
        {
            NeutralPronounSecondPersonSingular = string.Empty;
            NeutralPronounSecondPersonPlural = string.Empty;
            NeutralPronounSecondPersonPossessive = string.Empty;
            NeutralPronounFirstPersonSingular = string.Empty;
            NeutralPronounFirstPersonPossessive = string.Empty;
            ArticleDeterminant = string.Empty;
            ArticleNonDeterminant = string.Empty;
            Conjunction = string.Empty;
            VerbExistentialSingular = string.Empty;
            VerbExistentialPlural = string.Empty;
            PrepositionFar = string.Empty;
            PrepositionNear = string.Empty;
            PrepositionAttached = string.Empty;
            PrepositionOn = string.Empty;
            PrepositionInside = string.Empty;
            PrepositionAround = string.Empty;
            PrepositionOf = string.Empty;
        }

        #region Neutral Pronoun second-person singular/plural/possessive
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Neutral Pronoun Second Person Singular", Description = "A base word, the name says it all.")]
        [Required]
        public string NeutralPronounSecondPersonSingular { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Neutral Pronoun Second Person Plural", Description = "A base word, the name says it all.")]
        [Required]
        public string NeutralPronounSecondPersonPlural { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Neutral Pronoun Second Person Possessive", Description = "A base word, the name says it all.")]
        [Required]
        public string NeutralPronounSecondPersonPossessive { get; set; }
        #endregion

        #region Neutral pronoun first-person singular/possessive
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Neutral Pronoun First Person Singular", Description = "A base word, the name says it all.")]
        [Required]
        public string NeutralPronounFirstPersonSingular { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Neutral Pronoun First Person Possessive", Description = "A base word, the name says it all.")]
        [Required]
        public string NeutralPronounFirstPersonPossessive { get; set; }
        #endregion

        #region Article determinant/non
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Article Determinant", Description = "A base word, the name says it all.")]
        [Required]
        public string ArticleDeterminant { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Article Non-Determinant", Description = "A base word, the name says it all.")]
        [Required]
        public string ArticleNonDeterminant { get; set; }
        #endregion

        #region Conjunction
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Conjunction", Description = "A base word, the name says it all.")]
        [Required]
        public string Conjunction { get; set; }
        #endregion

        #region Verb existential singular/plural
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Verb Existential Singular", Description = "A base word, the name says it all.")]
        [Required]
        public string VerbExistentialSingular { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Verb Existential Plural", Description = "A base word, the name says it all.")]
        [Required]
        public string VerbExistentialPlural { get; set; }
        #endregion

        #region Verb prepositions far, near, attached, on, inside, around
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Preposition Far", Description = "A base word, the name says it all.")]
        [Required]
        public string PrepositionFar { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Preposition Near", Description = "A base word, the name says it all.")]
        [Required]
        public string PrepositionNear { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Preposition Attached", Description = "A base word, the name says it all.")]
        [Required]
        public string PrepositionAttached { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Preposition On", Description = "A base word, the name says it all.")]
        [Required]
        public string PrepositionOn { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Preposition Inside", Description = "A base word, the name says it all.")]
        [Required]
        public string PrepositionInside { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Preposition Around", Description = "A base word, the name says it all.")]
        [Required]
        public string PrepositionAround { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Preposition Of", Description = "A base word, the name says it all.")]
        [Required]
        public string PrepositionOf { get; set; }
        #endregion
    }
}
