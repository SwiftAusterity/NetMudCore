using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;

namespace NetMudCore.DataStructure.Inanimate
{
    /// <summary>
    /// "Object" entity
    /// </summary>
    public interface IInanimate : IActor, IInanimateFramework, ICanBeWorn, ICanBeHeld, IContains, ISpawnAsMultiple
    {
    }
}
