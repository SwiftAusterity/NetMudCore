﻿using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Locale;

namespace NetMudCore.DataStructure.Room
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    public interface IRoomTemplate : IRoomFramework, ILocationData, ISingleton<IRoom>
    {
        /// <summary>
        /// Gets the remaining distance to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) between here and there</returns>
        int GetDistanceDestination(ILocationData destination);

        /// <summary>
        /// What locale does this belong to
        /// </summary>
        ILocaleTemplate ParentLocation { get; set; }
    }
}
