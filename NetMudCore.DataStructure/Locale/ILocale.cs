using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.Zone;

namespace NetMudCore.DataStructure.Locale
{
    /// <summary>
    /// Collection of rooms in a zone
    /// </summary>
    public interface ILocale : ILocaleFramework, IEntity, IDiscoverable, ISpawnAsSingleton<ILocale>
    {
        /// <summary>
        /// The map of the rooms inside
        /// </summary>
        ILiveMap Interior { get; set; }

        /// <summary>
        /// The zone this lives in
        /// </summary>
        IZone ParentLocation { get; set; }

        /// <summary>
        /// Get all the rooms for the zone
        /// </summary>
        /// <returns>the rooms for the zone</returns>
        IEnumerable<IRoom> Rooms();

        /// <summary>
        /// Get the surrounding locations based on a strength radius
        /// </summary>
        /// <param name="strength">number of places to go out</param>
        /// <returns>list of valid surrounding locations</returns>
        IEnumerable<ILocation> GetSurroundings();

        /// <summary>
        /// Get the absolute center room of the locale
        /// </summary>
        /// <returns>the central room of the locale</returns>
        IRoom CentralRoom(int zIndex = -1);
    }
}
