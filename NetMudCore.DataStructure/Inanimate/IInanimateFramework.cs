using NetMudCore.DataStructure.Architectural.EntityBase;

namespace NetMudCore.DataStructure.Inanimate
{
    /// <summary>
    /// Backing data for "object"s
    /// </summary>
    public interface IInanimateFramework : ICanAccumulate, IDescribable
    {
        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; set; }
    }
}
