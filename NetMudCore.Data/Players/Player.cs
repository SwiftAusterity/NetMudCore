﻿using NetMudCore.CentralControl;
using NetMudCore.Combat;
using NetMudCore.Communication.Messaging;
using NetMudCore.Data.Architectural;
using NetMudCore.Data.Architectural.DataIntegrity;
using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataAccess.FileSystem;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Combat;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.Players;
using NetMudCore.DataStructure.System;
using NetMudCore.DataStructure.Zone;
using NetMudCore.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Players
{
    /// <summary>
    /// live player character entities
    /// </summary>
    [Serializable]
    [IgnoreAutomatedBackup]
    public class Player : EntityPartial, IPlayer
    {
        #region Template and Framework Values
        public override bool IsPlayer()
        {
            return true;
        }

        /// <summary>
        /// The name of the object in the data template
        /// </summary>

        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<IPlayerTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)PlayerDataCache.Get(new PlayerDataCacheKey(typeof(IPlayerTemplate), AccountHandle, TemplateId));
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
        public string SurName { get; set; }

        /// <summary>
        /// Has this character "graduated" from the tutorial yet
        /// </summary>
        public bool StillANoob { get; set; }

        /// <summary>
        /// The "user" level for commands and accessibility
        /// </summary>
        public StaffRank GamePermissionsRank { get; set; }

        public MobilityState StancePosition { get; set; }

        #endregion


        [JsonIgnore]
        private LiveCacheKey _descriptorKey;

        /// <summary>
        /// The connection the player is using to chat with us
        /// </summary>

        [JsonIgnore]
        public IDescriptor Descriptor
        {
            get
            {
                if (_descriptorKey == null)
                {
                    return default;
                }

                return LiveCache.Get<IDescriptor>(_descriptorKey);
            }

            set
            {
                _descriptorKey = new LiveCacheKey(value);

                PersistToCache();
            }
        }

        /// <summary>
        /// Type of connection this has, doesn't get saved as it's transitory information
        /// </summary>

        [JsonIgnore]
        public override IChannelType ConnectionType
        {
            get
            {
                //All player descriptors should be of ichanneltype too
                return (IChannelType)Descriptor;
            }
        }

        /// <summary>
        /// The account this character belongs to
        /// </summary>
        public string AccountHandle { get; set; }

        /// <summary>
        /// Sensory overrides for staff member characters
        /// </summary>
        public HashSet<MessagingType> SuperSenses { get; set; }

        /// <summary>
        /// fArt Combos
        /// </summary>
        public HashSet<IFightingArtCombination> Combos { get; set; }

        /// <summary>
        /// NPC's race data
        /// </summary>
        [Display(Name = "Race", Description = "Your genetic basis. Many races must be unlocked through specific means.")]
        public IRace Race { get; set; }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Player()
        {
            Inventory = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();
            Combos = new HashSet<IFightingArtCombination>();
            EnemyGroup = new HashSet<Tuple<IMobile, int>>();
            AllianceGroup = new HashSet<IMobile>();
            MobilesInside = new EntityContainer<IMobile>();
            Inventory = new EntityContainer<IInanimate>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="character">the backing data</param>
        public Player(IPlayerTemplate character)
        {
            Qualities = new HashSet<IQuality>();
            MobilesInside = new EntityContainer<IMobile>();
            Inventory = new EntityContainer<IInanimate>();
            TemplateId = character.Id;
            AccountHandle = character.AccountHandle;
            Combos = new HashSet<IFightingArtCombination>();
            EnemyGroup = new HashSet<Tuple<IMobile, int>>();
            AllianceGroup = new HashSet<IMobile>();
            GetFromWorldOrSpawn();
        }

        #region Connectivity Details
        /// <summary>
        /// Function used to close this connection
        /// </summary>
        public void CloseConnection()
        {
            Descriptor.Disconnect(string.Empty);
        }

        public override bool WriteTo(IEnumerable<string> output, bool delayed = false)
        {
            IEnumerable<string> strings = MessagingUtility.TranslateColorVariables(output.ToArray(), this);

            if (delayed)
            {
                var working = OutputBuffer.Any();

                //enforce the output buffer
                OutputBuffer.Add(strings);

                if (!working)
                {
                    Processor.StartSingeltonLoop(string.Format("PlayerOutputWriter_{0}", TemplateName), 1, 0, 2, () => SendOutput());
                }
            }
            else
            {
                return Descriptor.SendOutput(strings);
            }

            return true;
        }

        private bool SendOutput()
        {
            var sent = Descriptor.SendOutput(string.Join(" ", OutputBuffer.Select(cluster => string.Join(" ", cluster))));

            OutputBuffer.Clear();

            return sent;
        }
        #endregion
		
        #region health and combat
        /// <summary>
        /// Max stamina
        /// </summary>
        public int TotalStamina { get; set; }

        /// <summary>
        /// Max Health
        /// </summary>
        public int TotalHealth { get; set; }

        /// <summary>
        /// Current stamina for this
        /// </summary>
        public int CurrentStamina { get; set; }

        /// <summary>
        /// Current health for this
        /// </summary>
        public int CurrentHealth { get; set; }

        /// <summary>
        /// How much stagger this currently has
        /// </summary>

        [JsonIgnore]
        public int Stagger { get; set; }

        /// <summary>
        /// How much stagger resistance this currently has
        /// </summary>

        [JsonIgnore]
        public int Sturdy { get; set; }

        /// <summary>
        /// How off balance this is. Positive is forward leaning, negative is backward leaning, 0 is in balance
        /// </summary>

        [JsonIgnore]
        public int Balance { get; set; }

        /// <summary>
        /// What stance this is currently in (for fighting art combo choosing)
        /// </summary>
        public string Stance { get; set; }

        /// <summary>
        /// Is the current attack executing
        /// </summary>

        [JsonIgnore]
        public bool Executing { get; set; }

        /// <summary>
        /// Last attack executed
        /// </summary>

        [JsonIgnore]
        public IFightingArt LastAttack { get; set; }

        /// <summary>
        /// Last combo used for attacking
        /// </summary>

        [JsonIgnore]
        public IFightingArtCombination LastCombo { get; set; }

        /// <summary>
        /// Who you're primarily attacking
        /// </summary>

        [JsonIgnore]
        public IMobile PrimaryTarget { get; set; }

        /// <summary>
        /// Who you're fighting in general
        /// </summary>

        [JsonIgnore]
        public HashSet<Tuple<IMobile, int>> EnemyGroup { get; set; }

        /// <summary>
        /// Who you're support/tank focus is
        /// </summary>

        [JsonIgnore]
        public IMobile PrimaryDefending { get; set; }

        /// <summary>
        /// Who is in your group
        /// </summary>

        [JsonIgnore]
        public HashSet<IMobile> AllianceGroup { get; set; }


        /// <summary>
        /// Stop all aggression
        /// </summary>
        public void StopFighting()
        {
            LastCombo = null;
            LastAttack = null;
            Executing = false;
            PrimaryTarget = null;
            EnemyGroup = new HashSet<Tuple<IMobile, int>>();
        }

        /// <summary>
        /// Start a fight or switch targets forcibly
        /// </summary>
        /// <param name="victim"></param>
        public void StartFighting(IMobile victim)
        {
            var wasFighting = IsFighting();

            victim ??= this;

            if (victim != GetTarget())
            {
                PrimaryTarget = victim;
            }

            if (!EnemyGroup.Any(enemy => enemy.Item1 == PrimaryTarget))
            {
                EnemyGroup.Add(new Tuple<IMobile, int>(PrimaryTarget, 0));
            }

            if (!wasFighting)
            {
                Processor.StartSubscriptionLoop("Fighting", () => Round.ExecuteRound(this, victim), 50, false);
            }
        }

        /// <summary>
        /// Get the target to attack
        /// </summary>
        /// <returns>A target or self if shadowboxing</returns>
        public IMobile GetTarget()
        {
            var target = PrimaryTarget;

            //TODO: AI for NPCs for other branches
            if (PrimaryTarget == null || (PrimaryTarget.BirthMark.Equals(BirthMark) && EnemyGroup.Count > 0))
            {
                PrimaryTarget = EnemyGroup.OrderByDescending(enemy => enemy.Item2).FirstOrDefault()?.Item1;
                target = PrimaryTarget;
            }

            return target;
        }

        /// <summary>
        /// Is this actor in combat
        /// </summary>
        /// <returns>yes or no</returns>
        public bool IsFighting()
        {
            return GetTarget() != null;
        }

        public int Exhaust(int exhaustionAmount)
        {
            int stam = Sleep(-1 * exhaustionAmount);

            //TODO: Check for total exhaustion

            return stam;
        }

        public int Harm(int damage)
        {
            CurrentHealth = Math.Max(0, TotalHealth - damage);

            //TODO: Check for DEATH

            return CurrentHealth;
        }

        public int Recover(int recovery)
        {
            CurrentHealth = Math.Max(0, Math.Min(TotalHealth, TotalHealth + recovery));

            return CurrentHealth;
        }

        public int Sleep(int hours)
        {
            CurrentStamina = Math.Max(0, Math.Min(TotalStamina, TotalStamina + hours * 10));

            return CurrentStamina;
        }
        #endregion

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IPlayer GetLiveInstance()
        {
            return this;
        }

        public override void KickoffProcesses()
        {
            //quality degredation and stam/health regen
            Processor.StartSubscriptionLoop("Regeneration", Regen, 250, false);
            Processor.StartSubscriptionLoop("QualityDecay", QualityTimer, 500, false);
        }

        public bool Regen()
        {
            if(!IsFighting())
            {
                Recover(TotalHealth / 100);
            }

            Sleep(1);

            Descriptor.SendWrapper();
            return true;
        }

        public bool QualityTimer()
        {
            foreach (var quality in Qualities.Where(qual => qual.Value > 0))
            {
                SetQuality(-1, quality.Name, true);
            }

            Descriptor.SendWrapper();

            return true;
        }
		
        #region sensory range checks
        /// <summary>
        /// Gets the actual vision modifier taking into account blindness and other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetVisualRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperSenses.Contains(MessagingType.Visible))
            {
                return new ValueRange<float>(-999999, 999999);
            }

            int returnTop = 1;
            int returnBottom = 100;

            //TODO: Check for blindess/magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetAuditoryRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperSenses.Contains(MessagingType.Audible))
            {
                return new ValueRange<float>(-999999, 999999);
            }

            int returnTop = 1; //TODO: Add this to race or something
            int returnBottom = 100;

            //TODO: Check for magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetPsychicRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperSenses.Contains(MessagingType.Psychic))
            {
                return new ValueRange<float>(-999999, 999999);
            }

            int returnTop = 0; //TODO: Add this to race or something
            int returnBottom = 0;

            //TODO: Check for magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetTasteRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperSenses.Contains(MessagingType.Taste))
            {
                return new ValueRange<float>(-999999, 999999);
            }

            int returnTop = 1; //TODO: Add this to race or something
            int returnBottom = 100;

            //TODO: Check for magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetTactileRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperSenses.Contains(MessagingType.Tactile))
            {
                return new ValueRange<float>(-999999, 999999);
            }

            int returnTop = 1; //TODO: Add this to race or something
            int returnBottom = 100;

            //TODO: Check for magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetOlefactoryRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperSenses.Contains(MessagingType.Olefactory))
            {
                return new ValueRange<float>(-999999, 999999);
            }

            int returnTop = 1; //TODO: Add this to race or something
            int returnBottom = 100;

            //TODO: Check for magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            float lumins = 0;

            foreach (IInanimate dude in Inventory.EntitiesContained())
            {
                lumins += dude.GetCurrentLuminosity();
            }

            //TODO: Magical light, equipment, make inventory less bright depending on where it is

            return lumins;
        }
        #endregion

        #region Rendering
        #endregion

        #region Container
        /// <summary>
        /// Inanimates contained in this
        /// </summary>
        public IEntityContainer<IInanimate> Inventory { get; set; }

        /// <summary>
        /// Any mobiles (players, npcs) contained in this
        /// </summary>
        public IEntityContainer<IMobile> MobilesInside { get; set; }

        public int Capacity => 50;

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            List<T> contents = new();

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                contents.AddRange(MobilesInside.EntitiesContained().Select(ent => (T)ent));
            }

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                contents.AddRange(Inventory.EntitiesContained().Select(ent => (T)ent));
            }

            return contents;
        }

        /// <summary>
        /// Get all of the entities matching a type inside this in a named container
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        /// <param name="containerName">the name of the container</param>
        public IEnumerable<T> GetContents<T>(string containerName)
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            List<T> contents = new();

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                contents.AddRange(MobilesInside.EntitiesContained(containerName).Select(ent => (T)ent));
            }

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                contents.AddRange(Inventory.EntitiesContained(containerName).Select(ent => (T)ent));
            }

            return contents;
        }

        /// <summary>
        /// Move an entity into this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing)
        {
            return MoveInto(thing, string.Empty);
        }

        /// <summary>
        /// Move an entity into a named container in this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing, string containerName)
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                IInanimate obj = (IInanimate)thing;

                if (Inventory.Contains(obj, containerName))
                {
                    return "That is already in the container";
                }

                string moveError = MoveInto(obj);
                if (!string.IsNullOrWhiteSpace(moveError))
                {
                    return moveError;
                }

                Inventory.Add(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                IMobile obj = (IMobile)thing;

                if (MobilesInside.Contains(obj, containerName))
                {
                    return "That is already in the container";
                }

                string moveError = MoveInto(obj);
                if (!string.IsNullOrWhiteSpace(moveError))
                {
                    return moveError;
                }

                MobilesInside.Add(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            return "Invalid type to move to container.";
        }

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing)
        {
            return MoveFrom(thing, string.Empty);
        }

        /// <summary>
        /// Move an entity out of this' named container
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing, string containerName)
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                IInanimate obj = (IInanimate)thing;

                if (!Inventory.Contains(obj, containerName))
                {
                    return "That is not in the container";
                }

                Inventory.Remove(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                IMobile obj = (IMobile)thing;

                if (!MobilesInside.Contains(obj, containerName))
                {
                    return "That is not in the container";
                }

                MobilesInside.Remove(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            return "Invalid type to move from container.";
        }

        public IGlobalPosition GetContainerAsLocation()
        {
            return new GlobalPosition(CurrentLocation.CurrentZone, CurrentLocation.CurrentLocale, CurrentLocation.CurrentRoom) { CurrentContainer = this };
        }
        #endregion

        #region SpawnBehavior
        /// <summary>
        /// Tries to find this entity in the world based on its Id or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            IPlayer me = LiveCache.Get<IPlayer>(TemplateId);

            //Isn't in the world currently
            if (me == default(IPlayer))
            {
                SpawnNewInWorld();
            }
            else
            {
                IPlayerTemplate ch = me.Template<IPlayerTemplate>();
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                TemplateId = ch.Id;
                Inventory = me.Inventory;
                Keywords = me.Keywords;
                CurrentHealth = me.CurrentHealth;
                CurrentStamina = me.CurrentStamina;

                Qualities = me.Qualities;

                TotalHealth = me.TotalHealth;
                TotalStamina = me.TotalStamina;
                SurName = me.SurName;
                StillANoob = me.StillANoob;
                GamePermissionsRank = me.GamePermissionsRank;
                Combos = me.Combos;

                if (CurrentHealth == 0)
                {
                    CurrentHealth = ch.TotalHealth;
                }

                if (CurrentStamina == 0)
                {
                    CurrentStamina = ch.TotalStamina;
                }

                if (me.CurrentLocation == null)
                {
                    TryMoveTo(GetBaseSpawn());
                }
                else
                {
                    TryMoveTo((IGlobalPosition)me.CurrentLocation.Clone());
                }
            }
        }


        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            IPlayerTemplate ch = Template<IPlayerTemplate>();

            if (ch.CurrentLocation?.CurrentZone == null)
            {
                ch.CurrentLocation = GetBaseSpawn();
            }

            SpawnNewInWorld(ch.CurrentLocation);
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition position)
        {
            //We can't even try this until we know if the data is there
            IPlayerTemplate ch = Template<IPlayerTemplate>() ?? throw new InvalidOperationException("Missing backing data store on player spawn event.");

            Keywords = ch.Keywords;

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(ch);
                Birthdate = DateTime.Now;
            }

            Inventory = new EntityContainer<IInanimate>();

            Qualities = ch.Qualities;
            CurrentHealth = ch.TotalHealth;
            CurrentStamina = ch.TotalStamina;
            TotalHealth = ch.TotalHealth;
            TotalStamina = ch.TotalStamina;
            SurName = ch.SurName ?? "";
            StillANoob = ch.StillANoob;
            GamePermissionsRank = ch.GamePermissionsRank;
            Combos = new HashSet<IFightingArtCombination>(ch.Account.Config.Combos);

            IGlobalPosition spawnTo = position ?? GetBaseSpawn();

            //Set the data context's stuff too so we don't have to do this over again
            ch.CurrentLocation = spawnTo;
            ch.Save(ch.Account, StaffRank.Player); //characters/players dont actually need approval

            TryMoveTo(spawnTo);

            UpsertToLiveWorldCache(true);

            KickoffProcesses();

            Save();
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            string error = string.Empty;

            if (CurrentLocation?.CurrentLocation() != null)
            {
                error = CurrentLocation.CurrentLocation().MoveFrom(this);
            }

            //validate position
            if (newPosition != null && string.IsNullOrEmpty(error))
            {
                if (newPosition.CurrentLocation() != null)
                {
                    error = newPosition.CurrentLocation().MoveInto(this);
                }

                if (string.IsNullOrEmpty(error))
                {
                    CurrentLocation = newPosition;
                    UpsertToLiveWorldCache();
                    error = string.Empty;

                    IPlayerTemplate dt = Template<IPlayerTemplate>();
                    dt.CurrentLocation = newPosition;
                    dt.SystemSave();
	                dt.PersistToCache();
	                Save();
	                UpsertToLiveWorldCache(true);
                }
            }
            else
            {
                error = "Cannot move to an invalid location";
            }

            return error;
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Dimensions GetModelDimensions()
        {
            int height = Race.Head.Model.Height + Race.Torso.Model.Height + Race.Legs.Item.Model.Height;
            int length = Race.Torso.Model.Length;
            int width = Race.Torso.Model.Width;

            return new Dimensions(height, length, width);
        }

        /// <summary>
        /// Save this to the filesystem in Current
        /// </summary>
        /// <returns>Success</returns>
        public override bool Save()
        {
            try
            {
                PlayerData dataAccessor = new();
                dataAccessor.WriteOnePlayer(this);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Find the emergency we dont know where to spawn this guy spawn location
        /// </summary>
        /// <returns>The emergency spawn location</returns>
        private IGlobalPosition GetBaseSpawn()
        {
            //TODO
            int zoneId = StillANoob ? 0 : 0;

            return new GlobalPosition(LiveCache.Get<IZone>(zoneId));
        }

        public override object Clone()
        {
            throw new NotImplementedException("Can't clone player objects.");
        }
        #endregion
    }
}
