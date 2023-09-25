using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Zone;

namespace NetMudCore.DataStructure.Gaia
{

    /// <summary>
    /// Collector of locales, used for weather and herd patterning
    /// </summary>
    public interface IGaia : IEntity, IGaiaFramework, ISpawnAsSingleton<IGaia>
    {
        /// <summary>
        /// The current time of day (and month and year)
        /// </summary>
        ITimeOfDay CurrentTimeOfDay { get; set; }

        /// <summary>
        /// Where the planet is rotationally
        /// </summary>
        float PlanetaryRotation { get; set; }

        /// <summary>
        /// Where the planet is in its orbit
        /// </summary>
        float OrbitalPosition { get; set; }

        /// <summary>
        /// Collection of weather patterns for this world
        /// </summary>
        HashSet<MeterologicalFront> MeterologicalFronts { get; set; }

        /// <summary>
        /// Economic controller for this world
        /// </summary>
        IEconomy Macroeconomy { get; set; }

        /// <summary>
        /// Where the various celestial bodies are along their paths
        /// </summary>
        HashSet<ICelestialPosition> CelestialPositions { get; set; }

        /// <summary>
        /// Broadcast an event to the entire zone
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="sender">the sender</param>
        /// <param name="subject">the subject</param>
        /// <param name="target">the target</param>
        void BroadcastEvent(string message, IEntity sender, IEntity subject, IEntity target);

        /// <summary>
        /// Get the zones associated with this world
        /// </summary>
        /// <returns>list of zones</returns>
        IEnumerable<IZone> GetZones();
    }
}
