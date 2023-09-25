using NetMudCore.Commands.Attributes;
using NetMudCore.Communication.Messaging;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.NPC;
using System.Collections.Generic;

namespace NetMudCore.Commands.EntityManipulation
{
    [CommandKeyword("appraise", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IInanimate), CacheReferenceType.Inventory, false)]
    [CommandParameter(CommandUsage.Target, typeof(INonPlayerCharacter), CacheReferenceType.Direction, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Appraise : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Appraise()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            IInanimate thing = (IInanimate)Subject;

            if (Target == null)
            {
                RenderError("There is no merchant in that direction.");
                return false;
            }

            INonPlayerCharacter merchant = (INonPlayerCharacter)Target;

            if (merchant == null || (!merchant.DoIBuyThings()))
            {
                RenderError("There is no merchant that buys items in that direction.");
                return false;
            }

            int price = merchant.HaggleCheck(thing);

            if(price <= 0)
            {
                RenderError("The merchant will not buy that item.");
                return false;
            }

            ILexicalParagraph toActor = new LexicalParagraph(string.Format("The merchant appraises your {0} at {1}blz.", thing.GetDescribableName(Actor), price));

            ILexicalParagraph toArea = new LexicalParagraph("$T$ looks very closely at $A$'s $S$.");

            //TODO: language outputs
            Message messagingObject = new(toActor)
            {
                ToOrigin = new List<ILexicalParagraph> { toArea }
            };

            messagingObject.ExecuteMessaging(Actor, thing, merchant, OriginLocation.CurrentZone, null);

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
                "Valid Syntax: appraise &lt;direction&gt; &lt;item&gt;"
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
                return string.Format("Has the merchant appraise an item in your inventory for purchase.");
            }
            set { }
        }
    }
}
