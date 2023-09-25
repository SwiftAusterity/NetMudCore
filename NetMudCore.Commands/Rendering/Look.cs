﻿using NetMudCore.Commands.Attributes;
using NetMudCore.Communication.Messaging;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.Utility;
using System.Collections.Generic;

namespace NutMud.Commands.Rendering
{
    /// <summary>
    /// Invokes the current container's RenderToVisible
    /// </summary>
    [CommandKeyword("look", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(ILookable), new CacheReferenceType[] { CacheReferenceType.Entity }, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Look : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Look()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            _ = new List<string>();

            //Just do a blank execution as the channel will handle doing the room updates
            if (Subject == null)
            {
                //sb.AddRange(OriginLocation.CurrentLocation.RenderToVisible(Actor));

                ///Need to do like HMR with a simple "update UI" pipeline TODO
                Message blankMessenger = new(new LexicalParagraph("You observe your surroundings."));

                blankMessenger.ExecuteMessaging(Actor, (IEntity)Subject, null, OriginLocation.CurrentRoom, null);
                return true;
            }

            ILookable lookTarget = (ILookable)Subject;

            ILexicalParagraph toOrigin = new LexicalParagraph("$A$ looks at $T$.");

            ILexicalParagraph toSubject = new LexicalParagraph("$A$ looks at YOU.");

            Message messagingObject = new(lookTarget.RenderToVisible(Actor))
            {
                ToOrigin = new List<ILexicalParagraph> { toOrigin },
                ToSubject = new List<ILexicalParagraph> { toSubject }
            };

            messagingObject.ExecuteMessaging(Actor, (IEntity)Subject, null, OriginLocation.CurrentRoom, null);

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
                "Valid Syntax: look",
                "look &lt;target&gt;".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Look provides useful information about the location you are in or a target object or mobile.");
            }
            set { }
        }
    }
}
