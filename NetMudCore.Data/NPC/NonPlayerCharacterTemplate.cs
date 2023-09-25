﻿using NetMudCore.Data.Architectural.DataIntegrity;
using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.Data.NPC.IntelligenceControl;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.NPC;
using NetMudCore.DataStructure.NPC.IntelligenceControl;
using NetMudCore.Data.Architectural.ActorBase;
using System.ComponentModel.DataAnnotations;
using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataAccess.Cache;
using System.Text.Json.Serialization;
using NetMudCore.DataAccess;
using System.Collections.Generic;
using System;

namespace NetMudCore.Data.NPC
{
    /// <summary>
    /// Backing data for NPCs
    /// </summary>
    [Serializable]
    public class NonPlayerCharacterTemplate : EntityTemplatePartial, INonPlayerCharacterTemplate
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        [JsonIgnore]

        public override Type EntityClass
        {
            get { return typeof(NonPlayerCharacter); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>

        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]

        public override string[] Keywords
        {
            get
            {
                if (_keywords == null || _keywords.Length == 0)
                {
                    _keywords = new string[] { FullName().ToLower(), Name.ToLower(), SurName.ToLower() };
                }

                return _keywords;
            }
            set { _keywords = value; }
        }

        [JsonPropertyName("Gender")]
        private TemplateCacheKey _gender { get; set; }

        /// <summary>
        /// Gender data string for player characters
        /// </summary>

        [JsonIgnore]
        [NonNullableDataIntegrity("Gender is required.")]
        [Display(Name = "Gender", Description = "Your gender. You can submit new gender matrices on the dashboard.")]
        [DataType("GenderList")]
        [GenderDataBinder]
        [Required]
        public IGender Gender
        {
            get
            {
                return TemplateCache.Get<IGender>(_gender);
            }
            set
            {
                _gender = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// "family name" for NPCs
        /// </summary>
        [StringDataIntegrity("Gender is empty.")]
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Family Name", Description = "Last Name.")]
        [DataType(DataType.Text)]
        [Required]
        public string SurName { get; set; }

        [JsonPropertyName("Race")]
        private TemplateCacheKey _race { get; set; }

        /// <summary>
        /// What we're spawning
        /// </summary>
        [JsonIgnore]

        [NonNullableDataIntegrity("Race must be set.")]
        [Display(Name = "Race", Description = "The NPC's Race")]
        [UIHint("RaceList")]
        [RaceDataBinder]
        public IRace Race
        {
            get
            {
                return TemplateCache.Get<IRace>(_race);
            }
            set
            {
                _race = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// Max health for this
        /// </summary>
        public int TotalHealth { get; set; }

        /// <summary>
        /// Max stamina for this
        /// </summary>
        public int TotalStamina { get; set; }

        /// <summary>
        /// The matrix of preferences and AI details
        /// </summary>
        [UIHint("Personality")]
        public IPersonality Personality { get; set; }

        /// <summary>
        /// Base constructor
        /// </summary>
        public NonPlayerCharacterTemplate()
        {
            WillPurchase = new HashSet<IMerchandise>();
            WillSell = new HashSet<IMerchandise>();
            InventoryRestock = new HashSet<MerchandiseStock>();
            TeachableProficencies = new HashSet<IQuality>();
            Personality = new Personality();
            Race = new Race();
            Descriptives = new HashSet<ISensoryEvent>();
        }

        /// <summary>
        /// Full name to refer to this NPC with
        /// </summary>
        /// <returns>the full name string</returns>
        public string FullName()
        {
            return string.Format("{0} {1}", Name, SurName);
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Dimensions GetModelDimensions()
        {
            try
            {
                if (Race == null)
                {
                    return new Dimensions(0, 0, 0);
                }

                int height = (Race?.Head?.Model != null ? Race.Head.Model.Height : 0)
                            + (Race?.Torso?.Model != null ? Race.Torso.Model.Height : 0)
                            + (Race?.Legs?.Item?.Model != null ? Race.Legs.Item.Model.Height : 0);
                int length = Race?.Torso?.Model != null ? Race.Torso.Model.Length : 0;
                int width = Race?.Torso?.Model != null ? Race.Torso.Model.Width : 0;

                return new Dimensions(height, length, width);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return new Dimensions(0, 0, 0);
        }

        #region Merchant
        /// <summary>
        /// What this merchant is willing to purchase
        /// </summary>
        [Display(Name = "Purchase List", Description = "The item types the merchant is willing to purchase.")]
        [UIHint("BuyList")]
        public HashSet<IMerchandise> WillPurchase { get; set; }

        /// <summary>
        /// What this merchant is willing to sell
        /// </summary>
        [Display(Name = "Sell List", Description = "The item types the merchant is willing to sell out of their inventory.")]
        [UIHint("SellList")]
        public HashSet<IMerchandise> WillSell { get; set; }

        /// <summary>
        /// Inventory this merchant will generate on a timer Item, Quantity
        /// </summary>
        [Display(Name = "Restock Settings", Description = "The configuration for what items will be created to the merchant's inventory over time.")]
        [UIHint("InventoryRestockList")]
        public HashSet<MerchandiseStock> InventoryRestock { get; set; }
        #endregion

        #region Teacher
        /// <summary>
        /// Qualities this teacher can impart, the quality value is the max level it can be taught to (1 at a time)
        /// </summary>
        [Display(Name = "Proficency", Description = "The proficencies that this NPC can teach players up to the indicated level.")]
        [UIHint("TeachableProficencies")]
        public HashSet<IQuality> TeachableProficencies { get; set; }
        #endregion

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Race", Race.Name);
            returnList.Add("SurName", SurName);
            returnList.Add("Gender", Gender.Name);

            return returnList;
        }

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public override bool PersistToCache()
        {
            try
            {
                TemplateCache.Add(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new NonPlayerCharacterTemplate
            {
                Name = Name,
                Gender = Gender,
                WillSell = WillSell,
                WillPurchase = WillPurchase,
                TotalStamina = TotalStamina,
                TotalHealth = TotalHealth,
                SurName = SurName,
                Race = Race,
                Qualities = Qualities,
                Personality = Personality,
                InventoryRestock = InventoryRestock
            };
        }
    }
}
