using NetMudCore.DataStructure.Room;

namespace NetMudCore.DataStructure.Architectural.EntityBase
{
    public interface ILocationData : ITemplate
    {
        /// <summary>
        /// What pathways are affiliated with this (what it spawns with)
        /// </summary>
        IEnumerable<IPathwayTemplate> GetPathways(bool withReturn = false);

        /// <summary>
        /// What pathways lead to locales
        /// </summary>
        IEnumerable<IPathwayTemplate> GetLocalePathways(bool withReturn = false);

        /// <summary>
        /// What pathways lead to Zones
        /// </summary>
        IEnumerable<IPathwayTemplate> GetZonePathways(bool withReturn = false);
    }
}
