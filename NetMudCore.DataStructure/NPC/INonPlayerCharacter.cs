using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.ActorBase;

namespace NetMudCore.DataStructure.NPC
{
    /// <summary>
    /// NPC entity class
    /// </summary>
    public interface INonPlayerCharacter : IMobile, INonPlayerCharacterFramework, ISpawnAsMultiple, IThink, IAmAMerchant, IAmATeacher
    {
    }
}
