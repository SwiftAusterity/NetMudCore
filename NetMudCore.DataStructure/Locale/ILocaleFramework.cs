﻿using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Zone;

namespace NetMudCore.DataStructure.Locale
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface ILocaleFramework : IDiscoverableData, IDescribable, ISingleton<ILocale>
    {
        /// <summary>
        /// Get the basic map render for the zone
        /// </summary>
        /// <returns>the zone map in ascii</returns>
        string RenderMap(int zIndex, bool forAdmin = false);

        /// <summary>
        /// The diameter of the zone
        /// </summary>
        /// <returns>the diameter of the zone in room count</returns>
        Dimensions Diameter();

        /// <summary>
        /// Calculate the theoretical dimensions of the zone in inches
        /// </summary>
        /// <returns>the dimensions of the zone in inches</returns>
        Dimensions FullDimensions();

        /// <summary>
        /// Regenerate the internal map for the locale
        /// </summary>
        void RemapInterior();
    }
}
