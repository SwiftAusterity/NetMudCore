﻿using NetMudCore.Commands.Attributes;
using NetMudCore.Communication.Messaging;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using System.Collections.Generic;

namespace NetMudCore.Commands.System
{
    /// <summary>
    /// Invokes the current container's RenderToVisible
    /// </summary>
    [CommandQueueSkip]
    [CommandKeyword("stop", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class StopQ : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public StopQ()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            Message messagingObject = new(new LexicalParagraph("You STOP your current action and queue."));

            Actor.StopInput();

            messagingObject.ExecuteMessaging(Actor, null, null, null, null);

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
                "Valid Syntax: stop"
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
                return string.Format("Stop halts your currently executing action and flushes your pending command queue.");
            }
            set {  }
        }
    }
}
