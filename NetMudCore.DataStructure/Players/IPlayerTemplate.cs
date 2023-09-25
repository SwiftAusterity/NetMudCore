﻿using NetMudCore.DataStructure.Architectural.EntityBase;

namespace NetMudCore.DataStructure.Players
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    public interface IPlayerTemplate : ITemplate, IPlayerFramework
    {
        /// <summary>
        /// What account owns this character
        /// </summary>
        IAccount Account { get; }

        /// <summary>
        /// Last known location Id for character in live world
        /// </summary>
        IGlobalPosition CurrentLocation { get; set; }

        /// <summary>
        /// Given name + surname
        /// </summary>
        /// <returns></returns>
        string FullName();
    }
}
