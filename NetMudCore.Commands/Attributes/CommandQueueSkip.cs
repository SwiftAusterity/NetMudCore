using System;

namespace NetMudCore.Commands.Attributes
{
    /// <summary>
    /// Indicates this command skips the input buffer queue
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandQueueSkip : Attribute
    {
        /// <summary>
        /// Creates a new keyword attribute
        /// </summary>
        public CommandQueueSkip()
        {
        }
    }
}
