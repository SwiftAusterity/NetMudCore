using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;

namespace NetMudCore.DataStructure.Inanimate
{
    /// <summary>
    /// Backing data for "object"s
    /// </summary>
    public interface IInanimateTemplate : ITemplate, IInanimateFramework, ICanBeCrafted
    {
        /// <summary>
        /// Definition for the room's capacity for mobiles
        /// </summary>
        HashSet<IEntityContainerData<IMobile>> MobileContainers { get; set; }

        /// <summary>
        /// Definition for the room's capacity for inanimates
        /// </summary>
        HashSet<IEntityContainerData<IInanimate>> InanimateContainers { get; set; }
    }
}
