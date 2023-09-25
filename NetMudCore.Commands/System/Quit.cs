using NetMudCore.Commands.Attributes;
using NetMudCore.Communication.Messaging;
using NetMudCore.DataAccess.FileSystem;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Players;
using NetMudCore.Utility;
using System.Collections.Generic;

namespace NetMudCore.Commands.System
{
    /// <summary>
    /// Invokes the current container's RenderToVisible
    /// </summary>
    [CommandQueueSkip]
    [CommandKeyword("quit", false)]
    [CommandKeyword("exit", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Quit : CommandPartial
    {
         /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Quit()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            IPlayer player = (IPlayer)Actor;
            ILexicalParagraph toActor = new LexicalParagraph("You exit this reality.");

            ILexicalParagraph toOrigin = new LexicalParagraph("$A$ exits this reality.");


            Message messagingObject = new(toActor)
            {
                ToOrigin = new List<ILexicalParagraph> { toOrigin }
            };

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentZone, null);

            PlayerData playerDataWrapper = new();

            //Save the player out
            playerDataWrapper.WriteOnePlayer(player);
            player.CloseConnection();

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
                "Valid Syntax: quit",
                "exit".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Quit/Exit removes your character from the live game allowing you to leave safely or switch characters.");
            }
            set { }
        }
    }
}
