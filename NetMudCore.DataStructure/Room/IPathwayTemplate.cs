using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;

namespace NetMudCore.DataStructure.Room
{
    /// <summary>
    /// Backing data for Pathways
    /// </summary>
    public interface IPathwayTemplate : IPathwayFramework, ITemplate, IDescribable, ISingleton<IPathway>
    {
        /// <summary>
        /// The container this points into
        /// </summary>
        ILocationData Destination { get; set; }

        /// <summary>
        /// The container this starts in
        /// </summary>
        ILocationData Origin { get; set; }
    }
}
