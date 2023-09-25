using NetMudCore.Commands.Attributes;
using NetMudCore.Communication.Messaging;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.System;
using System.Collections.Generic;

namespace NetMudCore.Commands.EntityManipulation
{
    [CommandKeyword("drop", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IEntity), new CacheReferenceType[] { CacheReferenceType.Entity }, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Drop : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Drop()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            IEntity thing = (IEntity)Subject;
            IContains actor = (IContains)Actor;
            IContains place = (IContains)OriginLocation;

            actor.MoveFrom(thing);
            place.MoveInto(thing);

            ILexicalParagraph toActor = new LexicalParagraph("You drop $S$.");

            ILexicalParagraph toOrigin = new LexicalParagraph("$A$ drops $S$.");

            IMessage messagingObject = new Message(toActor)
            {
                ToOrigin = new List<ILexicalParagraph> { toOrigin }
            };

            messagingObject.ExecuteMessaging(Actor, thing, null, OriginLocation.CurrentRoom, null);

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
                "Valid Syntax: drop &lt;object&gt;"
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
                return string.Format("Drop moves an object from your inventory to the room you are currently in.");
            }
            set { }
        }
    }
}
