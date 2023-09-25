﻿using NetMudCore.Communication.Lexical;
using NetMudCore.Data.Architectural.DataIntegrity;
using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.Data.Linguistic;
using NetMudCore.DataAccess;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Locale;
using NetMudCore.DataStructure.Players;
using NetMudCore.DataStructure.Room;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Room
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    [Serializable]
    public class RoomTemplate : LocationTemplateEntityPartial, IRoomTemplate
    {
        /// <summary>
        /// The system type of data this attaches to
        /// </summary>

        [JsonIgnore]
        public override Type EntityClass
        {
            get { return typeof(Room); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [JsonIgnore]

        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Leader; } }

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
                    _keywords = new string[] { Name.ToLower() };
                }

                return _keywords;
            }
            set { _keywords = value; }
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        [NonNullableDataIntegrity("Physical Model is invalid.")]
        [UIHint("TwoDimensionalModel")]
        public IDimensionalModel Model { get; set; }

        [JsonPropertyName("Medium")]
        private TemplateCacheKey _medium { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>

        [JsonIgnore]
        [NonNullableDataIntegrity("Medium material is invalid.")]
        [Display(Name = "Medium", Description = "What the 'empty' space of the room is made of. (likely AIR, sometimes stone or dirt)")]
        [UIHint("MaterialList")]
        [MaterialDataBinder]
        public IMaterial Medium
        {
            get
            {
                return TemplateCache.Get<IMaterial>(_medium);
            }
            set
            {
                if (value != null)
                {
                    _medium = new TemplateCacheKey(value);
                }
            }
        }

        [JsonPropertyName("ParentLocation")]
        private TemplateCacheKey _parentLocation { get; set; }

        /// <summary>
        /// What zone this belongs to
        /// </summary>

        [JsonIgnore]
        [NonNullableDataIntegrity("Parent Location is invalid.")]
        [Display(Name = "Parent Locale", Description = "The locale this room belongs to.")]
        [UIHint("LocaleRoomDisplay")]
        public ILocaleTemplate ParentLocation
        {
            get
            {
                return TemplateCache.Get<ILocaleTemplate>(_parentLocation);
            }
            set
            {
                if (value != null)
                {
                    _parentLocation = new TemplateCacheKey(value);
                }
            }
        }

        /// <summary>
        /// Current coordinates of the room on its world map
        /// </summary>

        [JsonIgnore]
        public Coordinate Coordinates { get; set; }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public RoomTemplate()
        {
            Model = new DimensionalModel();
            Descriptives = new HashSet<ISensoryEvent>();
        }

        /// <summary>
        /// Spawn new room with its model
        /// </summary>
        [JsonConstructor]
        public RoomTemplate(DimensionalModel model)
        {
            Model = model;
            Descriptives = new HashSet<ISensoryEvent>();
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            IList<string> dataProblems = base.FitnessReport();

            if (Coordinates?.X < 0 || Coordinates?.Y < 0 || Coordinates?.Z < 0)
            {
                dataProblems.Add("Coordinates are invalid.");
            }

            return dataProblems;
        }

        /// <summary>
        /// Gets the remaining distance to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) between here and there</returns>
        public int GetDistanceDestination(ILocationData destination)
        {
            return -1;
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Dimensions GetModelDimensions()
        {
            return new Dimensions(Model.Height, Model.Length, Model.Width);
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IRoom GetLiveInstance()
        {
            return LiveCache.Get<IRoom>(Id);
        }

        public override IKeyedData Create(IAccount creator, StaffRank rank)
        {
            //approval will be handled inside the base call
            IKeyedData obj = base.Create(creator, rank);

            ParentLocation.RemapInterior();

            return obj;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Medium", Medium.Name);

            foreach (ISensoryEvent desc in Descriptives)
            {
                returnList.Add("Descriptives", string.Format("{0} ({1}): {2}", desc.SensoryType, desc.Strength, desc.Event.ToString()));
            }

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
                LexicalContext collectiveContext = new(null, null, null)
                {
                    Determinant = true,
                    Perspective = NarrativePerspective.ThirdPerson,
                    Plural = false,
                    Position = LexicalPosition.None,
                    Tense = LexicalTense.Present
                };

                List<IDictata> dictatas = new()
                {
                    new Dictata(new Linguistic.Lexica(LexicalType.ProperNoun, GrammaticalType.Subject, Name, collectiveContext))
                };
                dictatas.AddRange(Descriptives.Select(desc => desc.Event.GetDictata()));

                foreach (IDictata dictata in dictatas)
                {
                    LexicalProcessor.VerifyLexeme(dictata.GetLexeme());
                }

                TemplateCache.Add(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }
    }
}
