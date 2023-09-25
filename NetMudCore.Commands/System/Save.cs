using NetMudCore.Commands.Attributes;
using NetMudCore.Communication.Messaging;
using NetMudCore.DataAccess.FileSystem;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Players;
using System.Collections.Generic;

namespace NetMudCore.Commands.System
{
    /// <summary>
    /// Invokes the current container's RenderToVisible
    /// </summary>
    [CommandQueueSkip]
    [CommandKeyword("save", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Save : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Save()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            IPlayer player = (IPlayer)Actor;

            ILexicalParagraph toActor = new LexicalParagraph("You save your life.");

            Message messagingObject = new(toActor);

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentZone, null);

            PlayerData playerDataWrapper = new();

            //Save the player out
            playerDataWrapper.WriteOnePlayer(player);

            return true;
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new()
            {
                "Valid Syntax: save"
            };

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override MarkdownString HelpText
        {
            get
            {
                return string.Format("Save writes your character to the backup set. This also happens automatically behind the scenes quite often.");
            }
            set {  }
        }
    }
}
