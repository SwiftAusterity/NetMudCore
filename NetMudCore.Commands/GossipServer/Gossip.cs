﻿using NetMudCore.Commands.Attributes;
using NetMudCore.Communication.Messaging;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Players;
using NetMudCore.Utility;
using System.Collections.Generic;
using System.Linq;

namespace NetMudCore.Commands.GossipServer
{
    [CommandQueueSkip]
    [CommandKeyword("gossip", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(string), CacheReferenceType.String, "[a-zA-Z]+@[a-zA-Z]+$", true)]
    [CommandParameter(CommandUsage.Subject, typeof(string), CacheReferenceType.String, "@[a-zA-Z]+$", true)]
    [CommandParameter(CommandUsage.Target, typeof(string), CacheReferenceType.Greedy, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Gossip : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Gossip()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            List<string> sb = new();
            IPlayer playerActor = Actor.GetType().GetInterfaces().Contains(typeof(IPlayer)) ? Actor as IPlayer : null;

            if (playerActor != null && !playerActor.Template<IPlayerTemplate>().Account.Config.GossipSubscriber)
            {
                sb.Add(string.Format("You have disabled the Gossip network.", Subject));
            }
            else
            {
                string directTarget = string.Empty;
                string directTargetGame = string.Empty;

                if (Subject != null)
                {
                    string[] names = Subject.ToString().Split(new char[] { '@' });

                    if(names.Length == 2)
                    {
                        directTarget = names[0];
                        directTargetGame = names[1];
                    }
                    else if(names.Length == 1)
                    {
                        directTarget = names[0];
                    }
                }

                //GossipClient gossipClient = LiveCache.Get<GossipClient>("GossipWebClient");

                //string userName = Actor.TemplateName;

                //if (playerActor != null)
                //{
                //    userName = playerActor.AccountHandle;
                //}

                if (!string.IsNullOrWhiteSpace(directTarget) && !string.IsNullOrWhiteSpace(directTargetGame))
                {
                    //gossipClient.SendDirectMessage(userName, directTargetGame, directTarget, Target.ToString());
                    sb.Add(string.Format("You tell {1}@{2} '{0}'", Target, directTarget, directTargetGame));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(directTarget))
                    {
                        //gossipClient.SendMessage(userName, Target.ToString());
                        sb.Add(string.Format("You gossip '{0}'", Target));
                    }
                    else
                    {
                       // gossipClient.SendMessage(userName, Target.ToString(), directTarget);
                        sb.Add(string.Format("You {1} '{0}'", Target, directTarget));
                    }
                }
            }

            ILexicalParagraph toActor = new LexicalParagraph(sb.ToString());

            //TODO: language outputs
            Message messagingObject = new(toActor);

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
                "Valid Syntax: gossip &lt;text&gt;",
                "gossip @&lt;channel&gt; &lt;text&gt;".PadWithString(14, "&nbsp;", true),
                "gossip &lt;username&gt;@&lt;gamename&gt; &lt;text&gt;".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Gossip allows you to speak over the gossip inter-mud chat network.");
            }
            set { }
        }
    }
}
