﻿using NetMudCore.Commands.Attributes;
using NetMudCore.Communication.Messaging;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.System;
using NetMudCore.Utility;
using System.Collections.Generic;

namespace NetMudCore.Commands.Rendering
{
    [CommandKeyword("inventory", false, true, true)]
    [CommandKeyword("inv", false, false, true)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Inventory : CommandPartial
    {        
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Inventory()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            _ = new List<string>();
            IMobile chr = (IMobile)Actor;
            List<ILexicalParagraph> toActor = new()
            {
                new LexicalParagraph("You look through your belongings.")
            };

            foreach (IInanimate thing in chr.Inventory.EntitiesContained())
            {
                toActor.Add(thing.RenderAsContents(chr, new[] { MessagingType.Visible }));
            }

            ILexicalParagraph toOrigin = new LexicalParagraph("$A$ sifts through $G$ belongings.");

            Message messagingObject = new(toActor)
            {
                ToOrigin = new List<ILexicalParagraph> { toOrigin }
            };

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentRoom, null);

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
                "Valid Syntax: inventory",
                "inv".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Inventory lists out all inanimates currently on your person. It is an undetectable action unless a viewer has high perception.");
            }
            set { }
        }

    }
}
