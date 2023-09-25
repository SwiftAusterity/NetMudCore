using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.Zone;

namespace NetMudCore.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Locations are special containers for entities
    /// </summary>
    public interface ILocation : IContains, IEnvironment
    {
        /// <summary>
        /// Pathways leading out of (or into) this
        /// </summary>
        IEnumerable<IPathway> GetPathways(bool inward = false);

        /// <summary>
        /// Pathways leading out of (or into) this
        /// </summary>
        IEnumerable<IPathway> GetZonePathways(bool inward = false);

        /// <summary>
        /// Pathways leading out of (or into) this
        /// </summary>
        IEnumerable<IPathway> GetLocalePathways(bool inward = false);

        /// <summary>
        /// Mobiles (NPC, Players) in the room
        /// </summary>
        IEntityContainer<IMobile> MobilesInside { get; set; }

        /// <summary>
        /// IInanimate inventory
        /// </summary>
        IEntityContainer<IInanimate> Contents { get; set; }

        /// <summary>
        /// Get the surrounding locations based on a strength radius
        /// </summary>
        /// <param name="strength">number of places to go out</param>
        /// <returns>list of valid surrounding locations</returns>
        IEnumerable<ILocation> GetSurroundings(int strength);
    }
}
