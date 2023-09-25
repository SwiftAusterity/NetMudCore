﻿using NetMudCore.Communication;
using NetMudCore.Communication.Lexical;
using NetMudCore.Communication.Messaging;
using NetMudCore.Data.Architectural.Serialization;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataAccess.FileSystem;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.NPC.IntelligenceControl;
using NetMudCore.DataStructure.Players;
using NetMudCore.DataStructure.System;
using NetMudCore.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Architectural.EntityBase
{
    /// <summary>
    /// Abstract that tries to keep the entity classes cleaner
    /// </summary>
    [Serializable]
    public abstract class EntityPartial : SerializableDataPartial, IEntity
    {
        #region Data and live tracking properties
        /// <summary>
        /// Unique string for this live entity
        /// </summary>
        public string BirthMark { get; set; }

        /// <summary>
        /// When this entity was born to the world
        /// </summary>
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// An internal date for checking the last time this was saved
        /// </summary>

        [JsonIgnore]
        internal DateTime CleanUntil { get; set; } = DateTime.Now;

        /// <summary>
        /// The Id for the backing data
        /// </summary>
        public long TemplateId { get; set; }

        /// <summary>
        /// The name of the object in the data template
        /// </summary>

        [JsonIgnore]
        public abstract string TemplateName { get; }

        /// <summary>
        /// The backing data for this live entity
        /// </summary>
        public abstract T Template<T>() where T : IKeyedData;

        public virtual bool IsPlayer()
        {
            return false;
        }

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]

        private string[] _keywords;

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]

        public string[] Keywords
        {
            get
            {
                if (_keywords == null)
                {
                    if (Template<ITemplate>() == null)
                    {
                        if (string.IsNullOrWhiteSpace(TemplateName))
                        {
                            _keywords = Array.Empty<string>();
                        }
                        else
                        {
                            _keywords = new string[] { TemplateName.ToLower() };
                        }
                    }
                    else
                    {
                        _keywords = Template<ITemplate>().Keywords;
                    }
                }

                return _keywords;
            }
            set
            {
                _keywords = value;
            }
        }
        #endregion

        #region Connection Stuff
        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        public virtual bool WriteTo(IEnumerable<string> output, bool delayed = false)
        {
            //null output means send wrapper to players
            if(output == null)
            {
                if(IsPlayer())
                {
                    var thisPlayer = (IPlayer)this;

                    thisPlayer.Descriptor.SendWrapper();
                }

                return true;
            }

            IEnumerable<string> strings = MessagingUtility.TranslateColorVariables(output.ToArray(), this);

            return TriggerAIAction(strings);
        }

        private IChannelType _internalDescriptor;


        [JsonIgnore]
        public virtual IChannelType ConnectionType
        {
            get
            {
                _internalDescriptor ??= new InternalChannel();

                return _internalDescriptor;
            }
        }
        #endregion

        #region Ownership
        /// <summary>
        /// Who created this thing, their GlobalAccountHandle
        /// </summary>
        [JsonIgnore]

        public string CreatorHandle { get { return Template<ITemplate>().CreatorHandle; } }

        /// <summary>
        /// Who created this thing
        /// </summary>
        [JsonIgnore]

        public IAccount Creator { get { return Template<ITemplate>().Creator; } }

        /// <summary>
        /// The creator's account permissions level
        /// </summary>
        [JsonIgnore]

        public StaffRank CreatorRank { get { return Template<ITemplate>().CreatorRank; } }
        #endregion
        #region Command Queue
        /// <summary>
        /// Buffer of output to send to clients via WriteTo
        /// </summary>

        [JsonIgnore]
        public IList<IEnumerable<string>> OutputBuffer { get; set; }

        /// <summary>
        /// Buffer of command string input sent from the client
        /// </summary>

        [JsonIgnore]
        public IList<string> InputBuffer { get; set; }

        /// <summary>
        /// What is currently being executed
        /// </summary>

        [JsonIgnore]
        public string CurrentAction { get; set; }

        /// <summary>
        /// Stops whatever is being executed and clears the input buffer
        /// </summary>
        public void StopInput()
        {
            HaltInput();
            FlushInput();
        }

        /// <summary>
        /// Stops whatever is being executed, does not clear the input buffer
        /// </summary>
        public void HaltInput()
        {
            CurrentAction = string.Empty;
        }

        /// <summary>
        /// Clears the input buffer
        /// </summary>
        public void FlushInput()
        {
            InputBuffer = new List<string>();
        }

        /// <summary>
        /// Returns whats in the input buffer
        /// </summary>
        /// <returns>Any strings still in the input buffer</returns>
        public IEnumerable<string> PeekInput()
        {
            var newList = new List<string>() { string.Format("Acting: {0}", CurrentAction) };

            newList.AddRange(InputBuffer);

            return newList;
        }
        #endregion

        /// <summary>
        /// List of live qualities of this entity
        /// </summary>
        [UIHint("QualityList")]
        public HashSet<IQuality> Qualities { get; set; }

        /// <summary>
        /// Where in the live world this is
        /// </summary>
        [JsonConverter(typeof(JsonConverter<GlobalPosition>))]
        public IGlobalPosition CurrentLocation { get; set; }

        /// <summary>
        /// Check for a quality
        /// </summary>
        /// <param name="name">Gets the value of the request quality</param>
        /// <returns>The value</returns>
        public virtual int GetQuality(string name)
        {
            IQuality currentQuality = Qualities.FirstOrDefault(qual => qual.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (currentQuality == null)
            {
                return 0;
            }

            return currentQuality.Value;
        }

        /// <summary>
        /// Add a quality (can be negative)
        /// </summary>
        /// <param name="value">The value you're adding</param>
        /// <param name="additive">Is this additive or replace-ive</param>
        /// <returns>The new value</returns>
        public int SetQuality(int value, string quality, bool additive)
        {
            IQuality currentQuality = Qualities.FirstOrDefault(qual => qual.Name.Equals(quality, StringComparison.InvariantCultureIgnoreCase));

            if (currentQuality == null)
            {
                Qualities.Add(new Quality()
                {
                    Name = quality,
                    Type = QualityType.Aspect,
                    Visible = true,
                    Value = value
                });

                return value;
            }

            if (additive)
            {
                currentQuality.Value += value;
            }
            else
            {
                currentQuality.Value = value;
            }

            return value;
        }

        public EntityPartial()
        {
            OutputBuffer = new List<IEnumerable<string>>();
            InputBuffer = new List<string>();
        }

        #region Movement
        /// <summary>
        /// Move this inside of something
        /// </summary>
        /// <param name="container">The container to move into</param>
        /// <returns>was this thing moved?</returns>
        public virtual string TryMoveTo(IContains container)
        {
            return TryMoveTo(new GlobalPosition(container));
        }

        /// <summary>
        /// Change the position of this without physical movement
        /// </summary>
        /// <param name="newPosition">The new position the thing is in, will return with the original one if nothing moved</param>
        /// <returns>was this thing moved?</returns>
        public virtual string TryTeleport(IGlobalPosition newPosition)
        {
            return TryMoveTo(newPosition);
        }

        public abstract string TryMoveTo(IGlobalPosition newPosition);
        #endregion

        #region Caching
        /// <summary>
        /// What type of cache is this using
        /// </summary>
        [JsonIgnore]

        public virtual CacheType CachingType => CacheType.Live;

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public abstract void SpawnNewInWorld();

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public abstract void SpawnNewInWorld(IGlobalPosition spawnTo);

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool PersistToCache()
        {
            try
            {
                UpsertToLiveWorldCache();
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update this entry to the live world cache
        /// </summary>
        public void UpsertToLiveWorldCache(bool forceSave = false)
        {
            LiveCache.Add(this);

            DateTime now = DateTime.Now;

            if (CleanUntil < now.AddMinutes(-5) || forceSave)
            {
                CleanUntil = now;
                Save();
            }
        }
        #endregion

        #region Data Management
        /// <summary>
        /// Save this to the filesystem in Current
        /// </summary>
        /// <returns>Success</returns>
        public virtual bool Save()
        {
            try
            {
                //Dont save player inventories
                if (CurrentLocation?.CurrentContainer != null && CurrentLocation.CurrentContainer.ImplementsType<IPlayer>())
                {
                    return true;
                }

                LiveData dataAccessor = new();
                dataAccessor.WriteEntity(this);
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Save this to the filesystem in Current
        /// </summary>
        /// <returns>Success</returns>
        public virtual bool Remove()
        {
            try
            {
                LiveData dataAccessor = new();
                dataAccessor.RemoveEntity(this);
                LiveCache.Remove(new LiveCacheKey(this));
            }
            catch
            {
                return false;
            }

            return true;
        }
        #endregion
        /// <summary>
        /// For non-player entities - accepts output "shown" to it by the parser as a result of commands and events
        /// </summary>
        /// <param name="input">the output strings</param>
        /// <param name="trigger">the methodology type (heard, seen, etc)</param>
        /// <returns></returns>
        public bool TriggerAIAction(IEnumerable<string> input, AITriggerType trigger = AITriggerType.Seen)
        {
            //TODO: Actual AI code
            return true;
        }

        #region Generic Rendering
        /// <summary>
        /// Set of output relevant to this exit. These are essentially single word descriptions to render the path
        /// </summary>
        public HashSet<ISensoryEvent> Descriptives { get; set; }

        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the output</returns>
        public virtual ILexicalParagraph RenderToTrack(IEntity actor)
        {
            //Default for "tracking" is null
            return null;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes = null)
        {
            if (sensoryTypes == null || sensoryTypes.Length == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            IList<ISensoryEvent> Messages = new List<ISensoryEvent>();
            //Self becomes the first sense in the list
            foreach (MessagingType sense in sensoryTypes)
            {
                ISensoryEvent self = GetSelf(sense);

                switch (sense)
                {
                    case MessagingType.Audible:
                        self.Strength = GetAudibleDelta(viewer);

                        self.TryModify(GetAudibleDescriptives(viewer));
                        break;
                    case MessagingType.Olefactory:
                        self.Strength = GetOlefactoryDelta(viewer);

                        self.TryModify(GetOlefactoryDescriptives(viewer));
                        break;
                    case MessagingType.Psychic:
                        self.Strength = GetPsychicDelta(viewer);

                        self.TryModify(GetPsychicDescriptives(viewer));
                        break;
                    case MessagingType.Tactile:
                        self.Strength = GetTactileDelta(viewer);

                        self.TryModify(GetTouchDescriptives(viewer));
                        break;
                    case MessagingType.Taste:
                        self.Strength = GetTasteDelta(viewer);

                        self.TryModify(GetTasteDescriptives(viewer));
                        break;
                    case MessagingType.Visible:
                        self.Strength = GetVisibleDelta(viewer);

                        self.TryModify(GetVisibleDescriptives(viewer));
                        break;
                }

                if (self.Event.Modifiers.Count > 0)
                {
                    Messages.Add(self);
                }
            }

            return new LexicalParagraph(Messages);
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ISensoryEvent GetImmediateDescription(IEntity viewer, MessagingType sense)
        {
            ISensoryEvent me = GetSelf(sense);
            switch (sense)
            {
                case MessagingType.Audible:
                    me.Strength = GetAudibleDelta(viewer);

                    me.TryModify(GetAudibleDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Olefactory:
                    me.Strength = GetOlefactoryDelta(viewer);

                    me.TryModify(GetOlefactoryDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Psychic:
                    me.Strength = GetPsychicDelta(viewer);

                    me.TryModify(GetPsychicDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Tactile:
                    me.Strength = GetTactileDelta(viewer);

                    me.TryModify(GetTouchDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Taste:
                    me.Strength = GetTasteDelta(viewer);

                    me.TryModify(GetTasteDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Visible:
                    me.Strength = GetVisibleDelta(viewer);

                    me.TryModify(GetVisibleDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
            }

            return me;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string GetDescribableName(IEntity viewer)
        {
            short strength = GetVisibleDelta(viewer);

            return GetSelf(MessagingType.Visible, strength).ToString();
        }

        internal ISensoryEvent GetSelf(MessagingType type, short strength = 30)
        {
            return new SensoryEvent()
            {
                SensoryType = type,
                Strength = strength,
                Event = new Linguistic.Lexica() { Phrase = TemplateName, Type = LexicalType.ProperNoun, Role = GrammaticalType.Subject }
            };
        }
        #endregion

        #region Visual Rendering
        /// <summary>
        /// Gets the actual vision Range taking into account blindness and other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetVisualRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this visible to the viewer
        /// </summary>
        /// <param name="viewer">the viewing entity</param>
        /// <returns>(-100) to 100 rating of how well this can be detected. 0 is full detection. negative is too "low", over 0 is too "intense"</returns>
        public virtual short GetVisibleDelta(IEntity viewer, short modifier = 0)
        {
            if (viewer != null)
            {
                float value = GetCurrentLuminosity();
                ValueRange<float> range = viewer.GetVisualRange();

                float lowDelta = value - (range.Low - modifier);
                float highDelta = (range.High + modifier) - value;

                if(lowDelta < 0)
                {
                    return (short)Math.Max(-100, lowDelta);
                }

                if (highDelta < 0)
                {
                    return (short)Math.Min(100, Math.Abs(highDelta));
                }
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToVisible(IEntity viewer)
        {
            return GetFullDescription(viewer);
        }

        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        public virtual ILexicalParagraph RenderToScan(IEntity viewer)
        {
            //TODO: Make this half power
            return new LexicalParagraph(GetImmediateDescription(viewer, MessagingType.Visible));
        }

        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        public virtual ILexicalParagraph RenderToInspect(IEntity viewer)
        {
            //TODO: Make this double power
            return GetFullDescription(viewer);
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetVisibleDescriptives(IEntity viewer)
        {
            Descriptives ??= new HashSet<ISensoryEvent>();
            
            foreach(ISensoryEvent desc in Descriptives.Where(desc => desc.SensoryType == MessagingType.Visible))
            {
                short senseDelta = GetVisibleDelta(viewer, desc.Strength);

                yield return desc;
            }
        }
        #endregion

        #region Auditory Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetAuditoryRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>(-100) to 100 rating of how well this can be detected. 0 is full detection. negative is too "low", over 0 is too "intense"</returns>
        public virtual short GetAudibleDelta(IEntity viewer, short modifier = 0)
        {
            if (viewer != null)
            {
                float value = 30; //Something to do with material composition probably or vocal range?
                ValueRange<float> range = viewer.GetAuditoryRange();

                float lowDelta = value - (range.Low - modifier);
                float highDelta = (range.High + modifier) - value;

                if (lowDelta < 0)
                {
                    return (short)Math.Max(-100, lowDelta);
                }

                if (highDelta < 0)
                {
                    return (short)Math.Min(100, Math.Abs(highDelta));
                }
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToAudible(IEntity viewer)
        {
            ISensoryEvent self = GetSelf(MessagingType.Audible);
            self.Strength = GetAudibleDelta(viewer);
            self.TryModify(GetAudibleDescriptives(viewer));

            return new LexicalParagraph(self);
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetAudibleDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Audible);
        }
        #endregion

        #region Psychic (sense) Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetPsychicRange()
        {
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>(-100) to 100 rating of how well this can be detected. 0 is full detection. negative is too "low", over 0 is too "intense"</returns>
        public virtual short GetPsychicDelta(IEntity viewer, short modifier = 0)
        {
            if (viewer != null)
            {
                float value = 30; //Something to do with material composition or emotional state
                ValueRange<float> range = viewer.GetPsychicRange();

                float lowDelta = value - (range.Low - modifier);
                float highDelta = (range.High + modifier) - value;

                if (lowDelta < 0)
                {
                    return (short)Math.Max(-100, lowDelta);
                }

                if (highDelta < 0)
                {
                    return (short)Math.Min(100, Math.Abs(highDelta));
                }
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToPsychic(IEntity viewer)
        {
            ISensoryEvent self = GetSelf(MessagingType.Psychic);
            self.Strength = GetPsychicDelta(viewer);
            self.TryModify(GetPsychicDescriptives(viewer));

            return new LexicalParagraph(self);
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetPsychicDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Psychic);
        }
        #endregion

        #region Taste Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetTasteRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>(-100) to 100 rating of how well this can be detected. 0 is full detection. negative is too "low", over 0 is too "intense"</returns>
        public virtual short GetTasteDelta(IEntity viewer, short modifier = 0)
        {
            if (viewer != null)
            {
                float value = 30; //Something to do with material composition 
                ValueRange<float> range = viewer.GetTasteRange();

                float lowDelta = value - (range.Low - modifier);
                float highDelta = (range.High + modifier) - value;

                if (lowDelta < 0)
                {
                    return (short)Math.Max(-100, lowDelta);
                }

                if (highDelta < 0)
                {
                    return (short)Math.Min(100, Math.Abs(highDelta));
                }
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToTaste(IEntity viewer)
        {
            ISensoryEvent self = GetSelf(MessagingType.Taste);
            self.Strength = GetTasteDelta(viewer);

            self.TryModify(GetTasteDescriptives(viewer));

            return new LexicalParagraph(self);
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetTasteDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Taste);
        }
        #endregion

        #region Smell Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetOlefactoryRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>(-100) to 100 rating of how well this can be detected. 0 is full detection. negative is too "low", over 0 is too "intense"</returns>
        public virtual short GetOlefactoryDelta(IEntity viewer, short modifier = 0)
        {
            if (viewer != null)
            {
                float value = 30; //Something to do with material composition 
                ValueRange<float> range = viewer.GetOlefactoryRange();

                float lowDelta = value - (range.Low - modifier);
                float highDelta = (range.High + modifier) - value;

                if (lowDelta < 0)
                {
                    return (short)Math.Max(-100, lowDelta);
                }

                if (highDelta < 0)
                {
                    return (short)Math.Min(100, Math.Abs(highDelta));
                }
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToOlefactory(IEntity viewer)
        {
            ISensoryEvent self = GetSelf(MessagingType.Olefactory);
            self.Strength = GetOlefactoryDelta(viewer);
            self.TryModify(GetOlefactoryDescriptives(viewer));

            return new LexicalParagraph(self);
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetOlefactoryDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Olefactory);
        }
        #endregion

        #region Touch Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetTactileRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>(-100) to 100 rating of how well this can be detected. 0 is full detection. negative is too "low", over 0 is too "intense"</returns>
        public virtual short GetTactileDelta(IEntity viewer, short modifier = 0)
        {
            if (viewer != null)
            {
                float value = 30; //Something to do with material composition or emotional state
                ValueRange<float> range = viewer.GetTactileRange();

                float lowDelta = value - (range.Low - modifier);
                float highDelta = (range.High + modifier) - value;

                if (lowDelta < 0)
                {
                    return (short)Math.Max(-100, lowDelta);
                }

                if (highDelta < 0)
                {
                    return (short)Math.Min(100, Math.Abs(highDelta));
                }
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToTouch(IEntity viewer)
        {
            ISensoryEvent self = GetSelf(MessagingType.Tactile);
            self.Strength = GetTactileDelta(viewer);
            self.TryModify(GetTouchDescriptives(viewer));

            return new LexicalParagraph(self);
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetTouchDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Tactile);
        }
        #endregion

        #region Containment Rendering
        /// <summary>
        /// Render this as being show inside a container
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderAsContents(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (sensoryTypes == null || sensoryTypes.Length == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            //Add the existential modifiers
            return new LexicalParagraph(GetImmediateDescription(viewer, sensoryTypes[0]));
        }

        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="holder">entity holding the thing</param>
        /// <returns>the output</returns>
        public virtual ILexicalParagraph RenderAsHeld(IEntity viewer, IEntity holder)
        {
            return new LexicalParagraph(GetImmediateDescription(viewer, MessagingType.Visible));
        }

        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="wearer">entity wearing the item</param>
        /// <returns>the output</returns>

        public virtual ILexicalParagraph RenderAsWorn(IEntity viewer, IEntity wearer)
        {
            return new LexicalParagraph(new SensoryEvent()
            {
                SensoryType = MessagingType.Visible,
                Strength = 30,
                Event = new Linguistic.Lexica() { Phrase = TemplateName, Type = LexicalType.Noun, Role = GrammaticalType.Subject }
            });
        }
        #endregion  

        /// <summary>
        /// Renders HTML for the info card popups
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the output HTML</returns>
        public virtual string RenderToInfo(IEntity viewer)
        {
            if (viewer == null)
            {
                return string.Empty;
            }

            ITemplate dt = Template<ITemplate>();
            StringBuilder sb = new();
            StaffRank rank = viewer.ImplementsType<IPlayer>() ? viewer.Template<IPlayerTemplate>().GamePermissionsRank : StaffRank.Player;

            sb.Append("<div class='helpItem'>");

            sb.AppendFormat("<h3>{0}</h3>", GetDescribableName(viewer));
            sb.Append("<hr />");
            if (Qualities != null && Qualities.Count > 0)
            {
                sb.Append("<h4>Qualities</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", Qualities.Select(q => string.Format("({0}:{1})", q.Name, q.Value))));
            }

            sb.Append("</div>");

            return sb.ToString();
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public abstract float GetCurrentLuminosity();

        #region Processes
        public virtual void KickoffProcesses()
        {
        }
        #endregion

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(ILiveData other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                    {
                        return -1;
                    }

                    if (other.BirthMark.Equals(BirthMark))
                    {
                        return 1;
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILiveData other)
        {
            if (other != default(ILiveData))
            {
                try
                {
                    return other.GetType() == GetType() && other.BirthMark.Equals(BirthMark);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILiveData x, ILiveData y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(ILiveData obj)
        {
            return obj.GetType().GetHashCode() + obj.BirthMark.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + BirthMark.GetHashCode();
        }

        public static bool TryMoveDirection(MovementDirectionType direction, IGlobalPosition newPosition)
        {
            return true;
        }
        #endregion

        public abstract object Clone();

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public abstract Dimensions GetModelDimensions();

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public virtual float GetModelVolume()
        {
            Dimensions dimensions = GetModelDimensions();

            return Math.Max(1, dimensions.Height) * Math.Max(1, dimensions.Width) * Math.Max(1, dimensions.Length);
        }
    }
}
