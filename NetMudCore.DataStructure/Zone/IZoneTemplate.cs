using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Gaia;

namespace NetMudCore.DataStructure.Zone
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface IZoneTemplate : IZoneFramework, ILocationData, IEnvironmentData, ISingleton<IZone>
    {
        /// <summary>
        /// What world does this belong to
        /// </summary>
        IGaiaTemplate World { get; set; }

        /// <summary>
        /// Templates for generating randomized locales for zones
        /// </summary>
        HashSet<IAdventureTemplate> Templates { get; set; }

        /// <summary>
        /// Is this zone always discovered by players (ie no need to be discovered)
        /// </summary>
        bool AlwaysDiscovered { get; set; }
    }
}
