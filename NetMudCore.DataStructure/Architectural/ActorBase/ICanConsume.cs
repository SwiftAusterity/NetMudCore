using NetMudCore.DataStructure.Inanimate;

namespace NetMudCore.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// This entity can eat (for mobiles)
    /// </summary>
    public interface ICanConsume : IEat
    {
        /// <summary>
        /// What's in yo belly
        /// </summary>
        HashSet<IInanimate> StomachContents { get; set; }

        /// <summary>
        /// The act of eating
        /// </summary>
        /// <param name="food">the thing being eaten</param>
        /// <returns>current statiation</returns>
        int Consume(IInanimate food);
    }
}
