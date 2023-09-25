using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.System;

namespace NetMudCore.DataStructure.Room
{
    /// <summary>
    /// Entity for Pathways
    /// </summary>
    public interface IPathway : IPathwayFramework, IActor, ISpawnAsSingleton<IPathway>
    {
        /// <summary>
        /// Message cluster for entities entering
        /// </summary>
        IMessage Enter { get; set; }

        /// <summary>
        /// The container this points into
        /// </summary>
        ILocation Destination { get; set; }

        /// <summary>
        /// The container this starts in
        /// </summary>
        ILocation Origin { get; set; }
    }
}
