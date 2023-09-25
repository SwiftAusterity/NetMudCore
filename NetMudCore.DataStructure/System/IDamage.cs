using NetMudCore.DataStructure.Architectural.EntityBase;

namespace NetMudCore.DataStructure.System
{ 
    /// <summary>
    /// Helper class for inflicting damage to entities
    /// </summary>
    public interface IDamage
    {
        DamageType Type { get; set; }
    }
}
