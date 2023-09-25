﻿using NetMudCore.DataStructure.System;

namespace NetMudCore.DataStructure.Linguistic
{
    /// <summary>
    /// A gramatical element
    /// </summary>
    public interface ILexica : IComparable<ILexica>, IEquatable<ILexica>
    {
        /// <summary>
        /// The type of word this is to the sentence
        /// </summary>
        GrammaticalType Role { get; set; }

        /// <summary>
        /// The type of word this is in general
        /// </summary>
        LexicalType Type { get; set; }

        /// <summary>
        /// The actual word/phrase
        /// </summary>
        string Phrase { get; set; }

        /// <summary>
        /// Modifiers for this lexica. (Modifier, Conjunction)
        /// </summary>
        HashSet<ILexica> Modifiers { get; set; }

        /// <summary>
        /// The context for this, which gets passed downards to anything modifying it
        /// </summary>
        LexicalContext Context { get; set; }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(ILexica modifier, bool passthru = true);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(ILexica[] modifiers);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(IEnumerable<ILexica> modifiers);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(HashSet<ILexica> modifiers);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(LexicalType type, GrammaticalType role, string phrase, bool passthru = true);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(Tuple<LexicalType, GrammaticalType, string> modifier, bool passthru = true);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(Tuple<LexicalType, GrammaticalType, string>[] modifier);

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <param name="overridingContext">The full lexical context</param>
        /// <returns>A long description</returns>
        IEnumerable<ILexica> Unpack(MessagingType sensoryType, short strength, LexicalContext overridingContext);

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <returns>A long description</returns>
        IEnumerable<ILexica> Unpack(MessagingType sensoryType, short strength);

        /// <summary>
        /// Describe the lexica
        /// </summary>
        /// <param name="context">The full lexical context</param>
        /// <returns></returns>
        string Describe();

        /// <summary>
        /// Make a sentence out of this
        /// </summary>
        /// <param name="type">the sentence type</param>
        /// <returns>the sentence</returns>
        ILexicalSentence MakeSentence(SentenceType type, MessagingType sensoryType, short strength = 30);

        /// <summary>
        /// Get the dictata from this lexica
        /// </summary>
        /// <returns>A dictata</returns>
        IDictata GetDictata();

        /// <summary>
        /// Generates a new dictata
        /// </summary>
        /// <returns>the dictata</returns>
        IDictata GenerateDictata();

        /// <summary>
        /// Make a copy of this object
        /// </summary>
        /// <returns>A copy of the object</returns>
        ILexica Clone();
    }
}
