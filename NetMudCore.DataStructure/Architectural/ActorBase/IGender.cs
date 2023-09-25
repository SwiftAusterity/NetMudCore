﻿namespace NetMudCore.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Framework for entities having a gender
    /// </summary>
    public interface IGender : ILookupData
    {
        /// <summary>
        /// Is this a feminine gender for gramatical purposes
        /// </summary>
        bool Feminine { get; set; }

        /// <summary>
        /// Collective pronoun
        /// </summary>
        string Collective { get; set; }

        /// <summary>
        /// Possessive pronoun
        /// </summary>
        string Possessive { get; set; }

        /// <summary>
        /// Basic pronoun
        /// </summary>
        string Base { get; set; }

        /// <summary>
        /// Adult generalized noun "woman", "man"
        /// </summary>
        string Adult { get; set; }

        /// <summary>
        /// Child generalized noun "girl", "boy"
        /// </summary>
        string Child { get; set; }
    }
}
