﻿using NetMudCore.Data.Architectural;
using NetMudCore.Data.Architectural.DataIntegrity;
using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.Data.Architectural.Serialization;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Combat;
using NetMudCore.DataStructure.Players;
using NetMudCore.DataStructure.System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Players
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    [Serializable]
    [IgnoreAutomatedBackup]
    public class PlayerTemplate : EntityTemplatePartial, IPlayerTemplate
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>

        [JsonIgnore]
        public override Type EntityClass
        {
            get { return typeof(Player); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>

        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.None; } }

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
                    var surName = string.IsNullOrWhiteSpace(SurName) ? "" : SurName.ToLower();
                    _keywords = new string[] { FullName().ToLower(), Name.ToLower(), surName };
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
        /// "family name" for player character
        /// </summary>
        [StringDataIntegrity("Surname is required.")]
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Family Name", Description = "Last Name for you in-game.")]
        [DataType(DataType.Text)]
        public string SurName { get; set; }

        /// <summary>
        /// Has this character "graduated" from the tutorial yet
        /// </summary>
        public bool StillANoob { get; set; }

        /// <summary>
        /// The "user" level for commands and accessibility
        /// </summary>
        [Display(Name = "Chosen Role", Description = "The administrative role.")]
        [UIHint("EnumDropDownList")]
        public StaffRank GamePermissionsRank { get; set; }

        /// <summary>
        /// Sensory overrides for staff member characters
        /// </summary>
        [Display(Name = "Super Senses", Description = "What sensory ranges are maxed for testing purposes.")]
        [UIHint("SuperSenses")]
        public HashSet<MessagingType> SuperSenses { get; set; }

        [JsonPropertyName("RaceData")]
        private TemplateCacheKey _race { get; set; }

        /// <summary>
        /// The race data for the character
        /// </summary>

        [JsonIgnore]
        [NonNullableDataIntegrity("Missing racial data.")]
        [Display(Name = "Race", Description = "Your genetic basis. Many races must be unlocked through specific means.")]
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
        /// The last known location Id this character was seen in by system (for restore/backup purposes)
        /// </summary>
        [JsonConverter(typeof(JsonConverter<GlobalPosition>))]
        [NonNullableDataIntegrity("Missing location data.")]
        public IGlobalPosition CurrentLocation { get; set; }

        /// <summary>
        /// Account handle (user) this belongs to
        /// </summary>
        public string AccountHandle { get; set; }


        [JsonIgnore]
        private IAccount _account { get; set; }

        /// <summary>
        /// Account data object this is owned by
        /// </summary>

        [JsonIgnore]
        [NonNullableDataIntegrity("Missing account data.")]
        public IAccount Account
        {
            get
            {
                if (_account == null && !string.IsNullOrWhiteSpace(AccountHandle))
                {
                    _account = Players.Account.GetByHandle(AccountHandle);
                }

                return _account;
            }
        }

        public int TotalHealth { get; set; }
        public int TotalStamina { get; set; }

        /// <summary>
        /// fArt Combos
        /// </summary>
        public HashSet<IFightingArtCombination> Combos { get; set; }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public PlayerTemplate()
        {
            TotalHealth = 100;
            TotalStamina = 100;
            Qualities = new HashSet<IQuality>();
            Combos = new HashSet<IFightingArtCombination>();
            SuperSenses = new HashSet<MessagingType>();
        }

        [JsonConstructor]
        public PlayerTemplate(GlobalPosition currentLocation)
        {
            CurrentLocation = currentLocation;
            TotalHealth = 100;
            TotalStamina = 100;
            Qualities = new HashSet<IQuality>();
            SuperSenses = new HashSet<MessagingType>();
            Combos = new HashSet<IFightingArtCombination>();
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

                int height = Race.Head.Model.Height + Race.Torso.Model.Height + Race.Legs.Item.Model.Height;
                int length = Race.Torso.Model.Length;
                int width = Race.Torso.Model.Width;

                return new Dimensions(height, length, width);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return new Dimensions(0, 0, 0);
        }

        /// <summary>
        /// Full name to refer to this NPC with
        /// </summary>
        /// <returns>the full name string</returns>
        public string FullName()
        {
            return string.Format("{0} {1}", Name, SurName);
        }

        #region Caching
        /// <summary>
        /// What type of cache is this using
        /// </summary>
        public override CacheType CachingType => CacheType.PlayerData;

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public override bool PersistToCache()
        {
            try
            {
                PlayerDataCache.Add(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }
        #endregion

        #region data persistence
        /// <summary>
        /// Add it to the cache and save it to the file system
        /// </summary>
        /// <returns>the object with Id and other db fields set</returns>
        public override IKeyedData Create(IAccount creator, StaffRank rank)
        {
            DataAccess.FileSystem.PlayerData accessor = new();

            try
            {
                //reset this guy's Id to the next one in the list
                GetNextId();
                Created = DateTime.Now;
                Creator = creator;
                CreatorRank = rank;

                //Default godsight to all false on creation unless you're making a new administrator
                if (rank == StaffRank.Admin)
                {
                    SuperSenses = new HashSet<MessagingType>()
                    {
                        MessagingType.Audible,
                        MessagingType.Olefactory,
                        MessagingType.Psychic,
                        MessagingType.Tactile,
                        MessagingType.Taste,
                        MessagingType.Visible
                    };
                }

                //No approval stuff necessary here
                ApproveMe(creator, rank);

                PersistToCache();
                accessor.WriteCharacter(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return null;
            }

            return this;
        }

        /// <summary>
        /// Add it to the cache and save it to the file system made by SYSTEM
        /// </summary>
        /// <returns>the object with Id and other db fields set</returns>
        public override IKeyedData SystemCreate()
        {
            DataAccess.FileSystem.PlayerData accessor = new();

            try
            {
                if (Created != DateTime.MinValue)
                {
                    SystemSave();
                }
                else
                {

                    //reset this guy's Id to the next one in the list
                    GetNextId();
                    Created = DateTime.Now;
                    CreatorHandle = DataHelpers.SystemUserHandle;
                    CreatorRank = StaffRank.Builder;

                    PersistToCache();
                    accessor.WriteCharacter(this);
                }


                State = ApprovalState.Approved;
                ApproverHandle = DataHelpers.SystemUserHandle;
                ApprovedOn = DateTime.Now;
                ApproverRank = StaffRank.Builder;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return null;
            }

            return this;
        }

        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public override bool Remove(IAccount remover, StaffRank rank)
        {
            DataAccess.FileSystem.PlayerData accessor = new();

            try
            {
                //Remove from cache first
                PlayerDataCache.Remove(new PlayerDataCacheKey(this));

                //Remove it from the file system.
                accessor.ArchiveCharacter(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool Save(IAccount editor, StaffRank rank)
        {
            DataAccess.FileSystem.PlayerData accessor = new();

            try
            {
                //No approval stuff necessary here
                ApproveMe(editor, rank);

                LastRevised = DateTime.Now;

                PersistToCache();
                accessor.WriteCharacter(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool SystemSave()
        {
            DataAccess.FileSystem.PlayerData accessor = new();

            try
            {
                if (Created == DateTime.MinValue)
                {
                    SystemCreate();
                }
                else
                {
                    //only able to edit its own crap
                    if (CreatorHandle != DataHelpers.SystemUserHandle)
                    {
                        return false;
                    }

                    State = ApprovalState.Approved;
                    ApproverHandle = DataHelpers.SystemUserHandle;
                    ApprovedOn = DateTime.Now;
                    ApproverRank = StaffRank.Builder;
                    LastRevised = DateTime.Now;

                    PersistToCache();
                    accessor.WriteCharacter(this);
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
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
            return new PlayerTemplate
            {
                Name = Name,
                AccountHandle = AccountHandle,
                GamePermissionsRank = GamePermissionsRank,
                Gender = Gender,
                Qualities = Qualities,
                SurName = SurName,
                StillANoob = StillANoob,
                SuperSenses = SuperSenses,
                TotalHealth = TotalHealth,
                TotalStamina = TotalStamina,
                Race = Race
            };
        }
        #endregion
    }
}
