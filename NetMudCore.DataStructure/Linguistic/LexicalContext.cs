using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;

namespace NetMudCore.DataStructure.Linguistic
{
    /// <summary>
    /// Context for a lexica
    /// </summary>
    public class LexicalContext(IEntity observer, IGender genderForm, ILanguage language)
    {
        /// <summary>
        /// Render any subjects (proper nouns) into pronouns
        /// </summary>
        public bool Anonymize { get; set; } = false;

        /// <summary>
        /// The person/thing observing this
        /// </summary>
        public IEntity Observer { get; set; } = observer;

        /// <summary>
        /// The language this is derived from
        /// </summary>
        public ILanguage Language { get; set; } = language;

        /// <summary>
        /// Gender of the subject (for pronoun usage)
        /// </summary>
        public IGender GenderForm { get; set; } = genderForm;

        /// <summary>
        /// Chronological tense of word
        /// </summary>
        public LexicalTense Tense { get; set; } = LexicalTense.Present;

        /// <summary>
        /// Does this indicate some sort of relational positioning
        /// </summary>
        public LexicalPosition Position { get; set; } = LexicalPosition.None;

        /// <summary>
        /// Personage of the word
        /// </summary>
        public NarrativePerspective Perspective { get; set; } = NarrativePerspective.FirstPerson;

        /// <summary>
        /// Is this an determinant form or not (usually true)
        /// </summary>
        public bool Determinant { get; set; } = true;

        /// <summary>
        /// Is this a plural form
        /// </summary>
        public bool Plural { get; set; } = false;

        /// <summary>
        /// Is this a possessive form
        /// </summary>
        public bool Possessive { get; set; } = false;

        /// <summary>
        /// Tags that describe the purpose/meaning of the words
        /// </summary>
        public HashSet<string> Semantics { get; set; } = new HashSet<string>();

        /// <summary>
        /// Strength rating of word in relation to synonyms
        /// </summary>
        public int Severity { get; set; } = 0;

        /// <summary>
        /// Synonym rating for elegance
        /// </summary>
        public int Elegance { get; set; } = 0;

        /// <summary>
        /// Finesse synonym rating; execution of form
        /// </summary>
        public int Quality { get; set; } = 0;

        public LexicalContext Clone()
        {
            return new LexicalContext(Observer, GenderForm, Language)
            {
                Tense = Tense,
                Position = Position,
                Perspective = Perspective,
                Determinant = Determinant,
                Possessive = Possessive,
                Plural = Plural,
                Severity = Severity,
                Elegance = Elegance,
                Quality = Quality,
                Semantics = Semantics,
                Anonymize = Anonymize,
            };
        }
    }
}
