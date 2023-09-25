using NetMudCore.DataStructure.NPC.IntelligenceControl;

namespace NetMudCore.DataStructure.NPC
{
    /// <summary>
    /// Indicates an entity is subject to artificial NonPlayerCharacter triggers and can think for itself
    /// </summary>
    public interface IThink
    {
        IBrain Hypothalamus { get; set; }

        void DoTheThing(Motivator motivator);
    }
}
