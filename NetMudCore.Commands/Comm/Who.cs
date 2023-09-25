using NetMudCore.Commands.Attributes;
using NetMudCore.Communication.Messaging;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Players;
using System.Collections.Generic;
using System.Linq;

namespace NetMudCore.Commands.Comm
{
    [CommandQueueSkip]
    [CommandKeyword("who", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Who : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Who()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        internal override bool ExecutionBody()
        {
            IEnumerable<IPlayer> whoList = LiveCache.GetAll<IPlayer>().Where(player => player.Descriptor != null);

            ILexicalParagraph toActor = new LexicalParagraph(string.Join(",", whoList.Select(who => who.GetDescribableName(Actor))));

            Message messagingObject = new(toActor);

            messagingObject.ExecuteMessaging(Actor, null, null, null, null);

            return true;
        }

        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new()
            {
                "Valid Syntax: who"
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
                return string.Format("Check who is in the world currently.");
            }
            set { }
        }
    }
}
