using NetMudCore.DataStructure.Administrative;
using System;

namespace NetMudCore.Commands.Attributes
{
    /// <summary>
    /// Staff rank permissions for executing commands
    /// </summary>
    /// <remarks>
    /// Create a new permission attribute
    /// </remarks>
    /// <param name="minimumRankAllowed">Minimum staff rank a player must be before they can "see" and use this command</param>
    /// <param name="requiresTargetOwnership">Does the target require the zone be owned/allied to the Actor</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandPermissionAttribute(StaffRank minimumRankAllowed, bool requiresTargetOwnership = false) : Attribute
    {
        /// <summary>
        /// Minimum staff rank a player must be before they can "see" and use this command
        /// </summary>
        public StaffRank MinimumRank { get; private set; } = minimumRankAllowed;

        /// <summary>
        /// Does the target require the zone be owned/allied to the Actor
        /// </summary>
        public bool RequiresTargetOwnership { get; private set; }
    }
}
