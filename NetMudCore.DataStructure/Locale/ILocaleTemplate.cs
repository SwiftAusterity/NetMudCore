﻿using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.Zone;

namespace NetMudCore.DataStructure.Locale
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface ILocaleTemplate : ILocaleFramework, ITemplate, IDiscoverableData, ISingleton<ILocale>
    {
        /// <summary>
        /// The map of the rooms inside
        /// </summary>
        IMap Interior { get; set; }

        /// <summary>
        /// When this locale dies off, MinValue = never
        /// </summary>
        DateTime RollingExpiration { get; set; }

        /// <summary>
        /// The zone this belongs to
        /// </summary>
        IZoneTemplate ParentLocation { get; set; }

        /// <summary>
        /// The rooms contained within the locale should it need to regenerate from nothing
        /// </summary>
        IEnumerable<IRoomTemplate> Rooms();

        /// <summary>
        /// Get the absolute center room of the locale
        /// </summary>
        /// <returns>the central room of the locale</returns>
        IRoomTemplate CentralRoom(int zIndex = -1);
    }
}
