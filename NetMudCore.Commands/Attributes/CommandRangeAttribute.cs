using System;

namespace NetMudCore.Commands.Attributes
{
    /// <summary>
    /// Dictates the range at which a command can be used
    /// </summary>
    /// <remarks>
    /// Loads a new range attribute
    /// </remarks>
    /// <param name="type">Range type</param>
    /// <param name="value">The maximum range a command can target from</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class CommandRangeAttribute(CommandRangeType type, int value) : Attribute
    {
        /// <summary>
        /// The type of range we're looking at
        /// </summary>
        public CommandRangeType Type { get; private set; } = type;

        /// <summary>
        /// The maximum range a command can target from
        /// </summary>
        public int Value { get; private set; } = value;
    }

    /// <summary>
    /// The type of range we're looking at for command execution
    /// </summary>
    public enum CommandRangeType
    {
        Global,
        Regional,
        Local,
        Touch,
        Self
    }
}
