﻿using NetMudCore.Data.Architectural;
using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.PropertyValidation;
using NetMudCore.DataStructure.Players;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Players
{
    /// <summary>
    /// Messages to players
    /// </summary>
    [Serializable]
    public class PlayerMessage : ConfigData, IPlayerMessage
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>

        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.None; //Config data defaults to admin

        /// <summary>
        /// Type of configuation data this is
        /// </summary>

        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.Player;

        /// <summary>
        /// The unique key used to identify, store and retrieve data
        /// </summary>

        [JsonIgnore]
        public override string UniqueKey => string.Format("{0}_{1}_{2}", Name, RecipientName, Sent.ToBinary());

        [MarkdownStringLengthValidator(ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [Display(Name = "Body", Description = "The body of the message.")]
        [DataType("Markdown")]
        [MarkdownBinder]
        public MarkdownString Body { get; set; }

        /// <summary>
        /// Subject of the message
        /// </summary>
        [StringLength(255, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Subject", Description = "The subject of your message.")]
        [DataType(DataType.Text)]
        public string Subject { get; set; }

        /// <summary>
        /// When this was sent
        /// </summary>
        public DateTime Sent { get; set; }

        /// <summary>
        /// Name of the recipient character (can be blank)
        /// </summary>
        public string SenderName { get; set; }


        [JsonIgnore]
        private IAccount _sender { get; set; }

        /// <summary>
        /// Account data object this is owned by
        /// </summary>

        [JsonIgnore]
        public IAccount Sender
        {
            get
            {
                if (_sender == null && !string.IsNullOrWhiteSpace(SenderName))
                {
                    _sender = Account.GetByHandle(SenderName);
                }

                return _sender;
            }
            set
            {
                if (value != null)
                {
                    SenderName = value.GlobalIdentityHandle;
                    _sender = value;
                }
            }
        }


        [JsonIgnore]
        private IAccount _recipientAccount { get; set; }

        //Name = recipientAccountName
        /// <summary>
        /// The account recieving this
        /// </summary>

        [JsonIgnore]
        [Display(Name = "Recipient Account", Description = "The account you are sending this to.")]
        [DataType(DataType.Text)]
        public IAccount RecipientAccount
        {
            get
            {
                if (_recipientAccount == null && !string.IsNullOrWhiteSpace(Name))
                {
                    _recipientAccount = Account.GetByHandle(Name);
                }

                return _recipientAccount;
            }
            set
            {
                if (value != null)
                {
                    Name = value.GlobalIdentityHandle;
                    _recipientAccount = value;
                }
            }
        }

        /// <summary>
        /// Name of the recipient character (can be blank)
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// Recipeint character
        /// </summary>

        [JsonIgnore]
        [Display(Name = "Recipient", Description = "The character you are sending this to. Can be blank.")]
        [DataType(DataType.Text)]
        public IPlayerTemplate Recipient
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RecipientName))
                {
                    return null;
                }

                global::System.Collections.Generic.IEnumerable<IPlayerTemplate> characters = PlayerDataCache.GetAllForAccountHandle(Name);

                //TODO: Maybe get a character by name in cache
                return characters.FirstOrDefault(ch => ch.Name.Equals(RecipientName, StringComparison.InvariantCultureIgnoreCase));
            }
            set
            {
                if (value != null)
                {
                    RecipientName = value.Name;
                }
            }
        }

        /// <summary>
        /// Is this important? Does it make the UI bell ring
        /// </summary>
        public bool Important { get; set; }

        /// <summary>
        /// Has this been read yet?
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new PlayerMessage
            {
                Name = Name,
                Body = Body,
                Important = Important,
                Read = Read,
                Subject = Subject,
                Sent = Sent,
                SenderName = SenderName,
                Sender = Sender,
                RecipientName = RecipientName,
                RecipientAccount = RecipientAccount,
                Recipient = Recipient
            };
        }
    }
}
