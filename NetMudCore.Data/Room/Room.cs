﻿using NetMudCore.Communication.Messaging;
using NetMudCore.Data.Architectural;
using NetMudCore.Data.Architectural.DataIntegrity;
using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Gaia;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Locale;
using NetMudCore.DataStructure.NaturalResource;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.System;
using NetMudCore.DataStructure.Zone;
using NetMudCore.Gaia.Geographical;
using NetMudCore.Gaia.Meteorological;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.Room
{
    /// <summary>
    /// Places entities are (most of the time)
    /// </summary>
    [Serializable]
    public class Room : LocationEntityPartial, IRoom
    {
        /// <summary>
        /// The name of the object in the data template
        /// </summary>

        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<IRoomTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(IRoomTemplate), TemplateId));
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        [UIHint("TwoDimensionalModel")]
        public IDimensionalModel Model { get; set; }

        [JsonPropertyName("ParentLocation")]
        private LiveCacheKey _parentLocation { get; set; }

        /// <summary>
        /// The locale this belongs to
        /// </summary>

        [JsonIgnore]
        [NonNullableDataIntegrity("Rooms must have a locale affiliation.")]
        public ILocale ParentLocation
        {
            get
            {
                return LiveCache.Get<ILocale>(_parentLocation);
            }
            set
            {
                if (value != null)
                {
                    _parentLocation = new LiveCacheKey(value);
                }
            }
        }


        [JsonIgnore]
        private Coordinate _coordinates { get; set; }


        [JsonIgnore]
        [UIHint("Coordinate")]
        public Coordinate Coordinates
        {
            get
            {
                return _coordinates;
            }
            set
            {
                _coordinates = value;

                IRoomTemplate dt = Template<IRoomTemplate>();
                if (dt != null)
                {
                    dt.Coordinates = _coordinates;
                    dt.PersistToCache();
                }

            }
        }

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

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Dimensions GetModelDimensions()
        {
            return new Dimensions(Model.Height, Model.Length, Model.Width);
        }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Room()
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
            Coordinates = new Coordinate(-1, -1, -1);
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Room(IRoomTemplate room)
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
            Coordinates = new Coordinate(-1, -1, -1);

            TemplateId = room.Id;

            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Gets the remaining distance and next "step" to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) and the next path you'd have to use</returns>
        public Tuple<int, IPathway> GetDistanceAndNextStepDestination(ILocation destination)
        {
            int distance = -1;
            IPathway nextStep = null;

            return new Tuple<int, IPathway>(distance, nextStep);
        }

        /// <summary>
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        public override IEnumerable<ICelestial> GetVisibileCelestials(IEntity viewer)
        {
            IRoomTemplate dT = Template<IRoomTemplate>();
            IZone zone = CurrentLocation.CurrentZone;

            bool canSeeSky = IsOutside() && dT.Coordinates.Z >= zone.Template<IZoneTemplate>().BaseElevation;

            if (!canSeeSky)
                return Enumerable.Empty<ICelestial>();

            //The zone knows about the celestial positioning
            return zone.GetVisibileCelestials(viewer);
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            float lumins = 0;

            IZone zone = CurrentLocation.CurrentZone;
            zone?.GetCurrentLuminosity();

            foreach (IMobile dude in MobilesInside.EntitiesContained())
            {
                lumins += dude.GetCurrentLuminosity();
            }

            foreach (IInanimate thing in Contents.EntitiesContained())
            {
                lumins += thing.GetCurrentLuminosity();
            }

            return lumins;
        }

        public override IGlobalPosition GetContainerAsLocation()
        {
            return new GlobalPosition(CurrentLocation.CurrentZone, CurrentLocation.CurrentLocale, this);
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IRoom GetLiveInstance()
        {
            return this;
        }

        #region rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override ILexicalParagraph RenderToVisible(IEntity viewer)
        {
            return GetFullDescription(viewer);
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public override ILexicalParagraph GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes = null)
        {
            if (sensoryTypes == null || sensoryTypes.Length == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            LexicalContext collectiveContext = new(viewer, null, null)
            {
                Determinant = true,
                Perspective = NarrativePerspective.SecondPerson,
                Plural = false,
                Position = LexicalPosition.Around,
                Tense = LexicalTense.Present
            };

            LexicalContext discreteContext = new(viewer, null, null)
            {
                Determinant = true,
                Perspective = NarrativePerspective.ThirdPerson,
                Plural = false,
                Position = LexicalPosition.Attached,
                Tense = LexicalTense.Present
            };

            //Self becomes the first sense in the list
            List<ISensoryEvent> sensoryOutput = new();
            foreach (MessagingType sense in sensoryTypes)
            {
                ISensoryEvent me = GetSelf(sense);

                switch (sense)
                {
                    case MessagingType.Audible:
                        me.Strength = GetAudibleDelta(viewer);

                        IEnumerable<ISensoryEvent> aDescs = GetAudibleDescriptives(viewer);

                        me.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        ILexica uberSounds = new Linguistic.Lexica(LexicalType.Verb, GrammaticalType.Verb, "hear", collectiveContext);
                        uberSounds.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            ILexica newDesc = new Linguistic.Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberSounds.TryModify(newDesc);
                        }

                        if (uberSounds.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(uberSounds);
                        }

                        break;
                    case MessagingType.Olefactory:
                        me.Strength = GetOlefactoryDelta(viewer);

                        IEnumerable<ISensoryEvent> oDescs = GetOlefactoryDescriptives(viewer);

                        me.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        ILexica uberSmells = new Linguistic.Lexica(LexicalType.Verb, GrammaticalType.Verb, "smell", collectiveContext);
                        uberSmells.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            ILexica newDesc = new Linguistic.Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberSmells.TryModify(newDesc);
                        }

                        if (uberSmells.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(uberSmells);
                        }

                        break;
                    case MessagingType.Psychic:
                        me.Strength = GetPsychicDelta(viewer);

                        IEnumerable<ISensoryEvent> pDescs = GetPsychicDescriptives(viewer);

                        me.TryModify(pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        ILexica collectivePsy = new Linguistic.Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you", collectiveContext);

                        ILexica uberPsy = collectivePsy.TryModify(LexicalType.Verb, GrammaticalType.Verb, "sense");
                        uberPsy.TryModify(pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            ILexica newDesc = new Linguistic.Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberPsy.TryModify(newDesc);
                        }

                        if (uberPsy.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(collectivePsy);
                        }

                        break;
                    case MessagingType.Taste:
                        me.Strength = GetTasteDelta(viewer);

                        IEnumerable<ISensoryEvent> taDescs = GetPsychicDescriptives(viewer);

                        me.TryModify(taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        ILexica uberTaste = new Linguistic.Lexica(LexicalType.Verb, GrammaticalType.Verb, "taste", collectiveContext);
                        uberTaste.TryModify(taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            ILexica newDesc = new Linguistic.Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberTaste.TryModify(newDesc);
                        }

                        if (uberTaste.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(uberTaste);
                        }

                        break;
                    case MessagingType.Tactile:
                        me.Strength = GetTactileDelta(viewer);

                        IEnumerable<ISensoryEvent> tDescs = GetTouchDescriptives(viewer);

                        me.TryModify(tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        ILexica uberTouch = new Linguistic.Lexica(LexicalType.Verb, GrammaticalType.Verb, "feel", collectiveContext);
                        uberTouch.TryModify(tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            ILexica newDesc = new Linguistic.Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberTouch.TryModify(newDesc);
                        }

                        if (uberTouch.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(uberTouch);
                        }

                        //Add the temperature
                        me.TryModify(LexicalType.Verb, GrammaticalType.Verb, "feels").TryModify(new Linguistic.Lexica[] {
                            new Linguistic.Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertHumidityToType(EffectiveHumidity()).ToString(), collectiveContext),
                            new Linguistic.Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertTemperatureToType(EffectiveTemperature()).ToString(), collectiveContext)
                        });

                        break;
                    case MessagingType.Visible:
                        me.Strength = GetVisibleDelta(viewer);

                        IEnumerable<ISensoryEvent> vDescs = GetVisibleDescriptives(viewer);

                        me.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        ILexica uberSight = new Linguistic.Lexica(LexicalType.Verb, GrammaticalType.Verb, "see", collectiveContext);
                        uberSight.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            ILexica newDesc = new Linguistic.Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberSight.TryModify(newDesc);
                        }

                        if (uberSight.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(uberSight);
                        }

                        //Describe the size and population of this zone
                        DimensionalSizeDescription roomSize = GeographicalUtilities.ConvertSizeToType(GetModelDimensions(), GetType());

                        me.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, roomSize.ToString());

                        //Render people in the zone
                        CrowdSizeDescription populationSize = GeographicalUtilities.GetCrowdSize(GetContents<IMobile>().Count());

                        string crowdSize = "lonely";
                        if ((short)populationSize > (short)roomSize)
                        {
                            crowdSize = "crowded";
                        }
                        else if (populationSize > CrowdSizeDescription.Intimate)
                        {
                            crowdSize = "sparsely populated";
                        }

                        me.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, crowdSize);

                        break;
                }

                if (me != null)
                {
                    sensoryOutput.Add(me);
                }
            }

            foreach (ICelestial celestial in GetVisibileCelestials(viewer))
            {
                sensoryOutput.AddRange(celestial.RenderAsContents(viewer, sensoryTypes).Events);
            }

            foreach (INaturalResourceSpawn<IFauna> resource in FaunaNaturalResources)
            {
                sensoryOutput.AddRange(resource.Resource.RenderResourceCollection(viewer, resource.RateFactor).Events);
            }

            foreach (INaturalResourceSpawn<IFlora> resource in FloraNaturalResources)
            {
                sensoryOutput.AddRange(resource.Resource.RenderResourceCollection(viewer, resource.RateFactor).Events);
            }

            foreach (INaturalResourceSpawn<IMineral> resource in MineralNaturalResources)
            {
                sensoryOutput.AddRange(resource.Resource.RenderResourceCollection(viewer, resource.RateFactor).Events);
            }

            foreach (IPathway path in GetPathways())
            {
                sensoryOutput.AddRange(path.RenderAsContents(viewer, sensoryTypes).Events);
            }

            foreach (IInanimate obj in GetContents<IInanimate>())
            {
                sensoryOutput.AddRange(obj.RenderAsContents(viewer, sensoryTypes).Events);
            }

            foreach (IMobile mob in GetContents<IMobile>().Where(player => !player.Equals(viewer)))
            {
                sensoryOutput.AddRange(mob.RenderAsContents(viewer, sensoryTypes).Events);
            }

            return new LexicalParagraph(sensoryOutput);
        }

        /// <summary>
        /// Renders out an ascii map of this room plus all rooms in the radius
        /// </summary>
        /// <param name="radius">how far away to render</param>
        /// <returns>the string</returns>
        public string RenderCenteredMap(int radius, bool visibleOnly)
        {
            //TODO: fix visibility
            return Cartography.Rendering.RenderRadiusMap(this, 3, visibleOnly);
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Tries to find this entity in the world based on its Id or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            IRoom me = LiveCache.Get<IRoom>(TemplateId, typeof(RoomTemplate));

            //Isn't in the world currently
            if (me == default(IRoom))
            {
                SpawnNewInWorld();
            }
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition(this));
        }


        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            IRoomTemplate bS = Template<IRoomTemplate>() ?? throw new InvalidOperationException("Missing backing data store on room spawn event.");

            Keywords = new string[] { bS.Name.ToLower() };
            Model = bS.Model;
            Descriptives = bS.Descriptives;
            Qualities = bS.Qualities;

            FloraNaturalResources ??= new HashSet<INaturalResourceSpawn<IFlora>>();

            FaunaNaturalResources ??= new HashSet<INaturalResourceSpawn<IFauna>>();

            MineralNaturalResources ??= new HashSet<INaturalResourceSpawn<IMineral>>();

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            UpsertToLiveWorldCache(true);

            ParentLocation = LiveCache.Get<ILocale>(bS.ParentLocation.Id);
            spawnTo.CurrentLocale = ParentLocation;
            spawnTo.CurrentZone = ParentLocation.ParentLocation;

            if (spawnTo?.CurrentLocale == null || spawnTo?.CurrentZone == null)
            {
                spawnTo = new GlobalPosition(this);
            }

            CurrentLocation = spawnTo;

            UpsertToLiveWorldCache(true);

            Save();
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
