﻿using NetMudCore.DataStructure.Administrative;

namespace NetMudCore.DataStructure.Players
{
    public interface IAccount : IComparable<IAccount>, IEquatable<IAccount>, IEqualityComparer<IAccount>
    {
        /// <summary>
        /// Unique string key for player user accounts
        /// </summary>
        string GlobalIdentityHandle { get; set; }

        /// <summary>
        /// What log channels a player is subscribe to listen to
        /// </summary>
        IList<string> LogChannelSubscriptions { get; set; }

        /// <summary>
        /// What characters this account owns
        /// </summary>
        HashSet<IPlayerTemplate> Characters { get; set; }

        /// <summary>
        /// The config values for this account
        /// </summary>
        IAccountConfig Config { get; set; }

        /// <summary>
        /// Id for currently selected character
        /// </summary>
        long CurrentlySelectedCharacter { get; set; }

        /// <summary>
        /// Get the current character for someone
        /// </summary>
        /// <returns></returns>
        IPlayerTemplate GetCurrentlySelectedCharacter();

        /// <summary>
        /// Add a character to this user
        /// </summary>
        /// <param name="newCharacter">the character to add</param>
        /// <returns>success status</returns>
        string AddCharacter(IPlayerTemplate newCharacter);

        /// <summary>
        /// Delete this account
        /// </summary>
        /// <param name="remover">The person removing this account</param>
        /// <param name="removerRank">The remover's staff ranking</param>
        /// <returns>success</returns>
        bool Delete(IAccount remover, StaffRank removerRank);
    }
}
