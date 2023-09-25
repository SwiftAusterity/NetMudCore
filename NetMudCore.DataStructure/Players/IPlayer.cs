using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.System;

namespace NetMudCore.DataStructure.Players
{
    /// <summary>
    /// Player character + account entity class
    /// </summary>
    public interface IPlayer : IMobile, IPlayerFramework, ISpawnAsSingleton<IPlayer>
    {
        /// <summary>
        /// Function used to close the connection
        /// </summary>
        void CloseConnection();

        /// <summary>
        /// How this player is connected
        /// </summary>
        IDescriptor Descriptor { get; set; }
    }
}
