using NetMudCore.DataStructure.Linguistic;

namespace NetMudCore.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Indicates a data structure has additional descriptives, is part of rendering
    /// </summary>
    public interface IDescribable
    {
        /// <summary>
        /// Set of output relevant to this exit
        /// </summary>
        HashSet<ISensoryEvent> Descriptives { get; set; }
    }
}
