﻿using NetMudCore.Authentication;
using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural.ActorBase;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Combat;
using NetMudCore.DataStructure.Gaia;
using NetMudCore.DataStructure.Gossip;
using NetMudCore.DataStructure.Inanimate;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.Locale;
using NetMudCore.DataStructure.NaturalResource;
using NetMudCore.DataStructure.NPC;
using NetMudCore.DataStructure.Players;
using NetMudCore.DataStructure.Room;
using NetMudCore.DataStructure.System;
using NetMudCore.DataStructure.Zone;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;

namespace NetMudCore.Models.Admin
{
    public class DashboardViewModel : IBaseViewModel
    {
        public ApplicationUser? AuthedUser { get; set; }

        public DashboardViewModel()
        {
            Inanimates = Enumerable.Empty<IInanimateTemplate>();
            Rooms = Enumerable.Empty<IRoomTemplate>();
            NPCs = Enumerable.Empty<INonPlayerCharacterTemplate>();
            Zones = Enumerable.Empty<IZoneTemplate>();
            Locales = Enumerable.Empty<ILocaleTemplate>();
            Worlds = Enumerable.Empty<IGaiaTemplate>();
            FightingArts = Enumerable.Empty<IFightingArt>();
            DimensionalModels = Enumerable.Empty<IDimensionalModelData>();
            HelpFiles = Enumerable.Empty<IHelp>();
            Materials = Enumerable.Empty<IMaterial>();
            Races = Enumerable.Empty<IRace>();
            Flora = Enumerable.Empty<IFlora>();
            Fauna = Enumerable.Empty<IFauna>();
            Minerals = Enumerable.Empty<IMineral>();
            UIModules = Enumerable.Empty<IUIModule>();
            Celestials = Enumerable.Empty<ICelestial>();
            Journals = Enumerable.Empty<IJournalEntry>();
            Genders = Enumerable.Empty<IGender>();

            DictionaryWords = Enumerable.Empty<ILexeme>();
            Languages = Enumerable.Empty<ILanguage>();

            LiveWorlds = 0;
            LiveZones = 0;
            LiveLocales = 0;
            LiveRooms = 0;
            LiveInanimates = 0;
            LiveNPCs = 0;

            LivePlayers = 0;
        }

        //Backing Data
        public IEnumerable<IRoomTemplate> Rooms { get; set; }
        public IEnumerable<IInanimateTemplate> Inanimates { get; set; }
        public IEnumerable<INonPlayerCharacterTemplate> NPCs { get; set; }
        public IEnumerable<IZoneTemplate> Zones { get; set; }
        public IEnumerable<ILocaleTemplate> Locales { get; set; }
        public IEnumerable<IGaiaTemplate> Worlds { get; set; }

        //Lookup Data
        public IEnumerable<IDimensionalModelData> DimensionalModels { get; set; }
        public IEnumerable<IHelp> HelpFiles { get; set; }
        public IEnumerable<IMaterial> Materials { get; set; }
        public IEnumerable<IRace> Races { get; set; }
        public IEnumerable<IFlora> Flora { get; set; }
        public IEnumerable<IFauna> Fauna { get; set; }
        public IEnumerable<IFightingArt> FightingArts { get; set; }
        public IEnumerable<IMineral> Minerals { get; set; }
        public IEnumerable<IUIModule> UIModules { get; set; }
        public IEnumerable<ICelestial> Celestials { get; set; }
        public IEnumerable<IJournalEntry> Journals { get; set; }
        public IEnumerable<IGender> Genders { get; set; }

        //Config Data
        public IEnumerable<ILexeme> DictionaryWords { get; set; }
        public IEnumerable<ILanguage> Languages { get; set; }

        //Running Data
        public Dictionary<string, CancellationTokenSource> LiveTaskTokens { get; set; }

        public int LiveWorlds { get; set; }
        public int LiveZones { get; set; }
        public int LiveLocales { get; set; }
        public int LiveRooms { get; set; }
        public int LiveInanimates { get; set; }
        public int LiveNPCs { get; set; }
        public int LivePlayers { get; set; }

        [Display(Name = "Websocket Portal Available", Description = "Are new connections being accepted over websockets?")]
        public bool WebsocketPortalActive { get; set; }

        [Display(Name = "System Language", Description = "The default language used for the system.")]
        [DataType(DataType.Text)]
        public string SystemLanguage { get; set; }

        [Display(Name = "User Creation", Description = "Are new users allowed to register?")]
        [UIHint("Boolean")]
        public bool UserCreationActive { get; set; }

        [Display(Name = "Admins Only", Description = "Are only admins allowed to log in - noone at StaffRank.Player?")]
        [UIHint("Boolean")]
        public bool AdminsOnly { get; set; }

        [Display(Name = "Live Translation", Description = "Do new Dictata get translated to the UI languages?")]
        [UIHint("Boolean")]
        public bool TranslationActive { get; set; }

        [Display(Name = "Azure API Key", Description = "The API key for your azure translation service.")]
        [DataType(DataType.Text)]
        public string AzureTranslationKey { get; set; }

        [Display(Name = "Deep Lex", Description = "Do words get deep lexed through Mirriam Webster?")]
        [UIHint("Boolean")]
        public bool DeepLexActive { get; set; }

        [Display(Name = "Mirriam Dictionary Key", Description = "The API key for your mirriam webster dictionary service.")]
        [DataType(DataType.Text)]
        public string MirriamDictionaryKey { get; set; }

        [Display(Name = "Mirriam Thesaurus Key", Description = "The API key for your mirriam webster thesaurus service.")]
        [DataType(DataType.Text)]
        public string MirriamThesaurusKey { get; set; }

        [Display(Name = "Base Language", Description = "The base language for the system.")]
        [UIHint("LanguageList")]
        [LanguageDataBinder]
        public ILanguage BaseLanguage { get; set; }

        [Display(Name = "Death Coordinate X", Description = "The coordinates you recall to on death.")]
        [DataType(DataType.Text)]
        [Required]
        public int DeathRecallCoordinateX { get; set; }

        [Display(Name = "Death Coordinate Y", Description = "The coordinates you recall to on death.")]
        [DataType(DataType.Text)]
        [Required]
        public int DeathRecallCoordinateY { get; set; }

        [Display(Name = "Subject", Description = "The subject of the death notice notification sent on death.")]
        [DataType(DataType.Text)]
        [Required]
        public string DeathNoticeSubject { get; set; }

        [Display(Name = "From", Description = "The from field for the death notice.")]
        [DataType(DataType.Text)]
        [Required]
        public string DeathNoticeFrom { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType("Markdown")]
        [Display(Name = "Body", Description = "The body of the death notice.")]
        [Required]
        public string DeathNoticeBody { get; set; }

        [Display(Name = "Quality", Description = " Should any qualities of the player change on death (like money removal).")]
        [DataType(DataType.Text)]
        public string[] QualityChange { get; set; }

        [Display(Name = "Value", Description = " Should any qualities of the player change on death (like money removal).")]
        [DataType(DataType.Text)]
        public int[] QualityChangeValue { get; set; }

        //Gossip Configuration
        [Display(Name = "Server Active", Description = "Is gossip supposed to be on?")]
        [UIHint("Boolean")]
        public bool GossipActive { get; set; }

        [Display(Name = "Client Id", Description = "The ID for this gossip client.")]
        [DataType(DataType.Text)]
        public string ClientId { get; set; }

        [Display(Name = "Client Secret", Description = "The client secret to share for auth.")]
        [DataType(DataType.Text)]
        public string ClientSecret { get; set; }

        [Display(Name = "Name", Description = "The name this sends to gossip to represent itself.")]
        [DataType(DataType.Text)]
        public string ClientName { get; set; }

        [Display(Name = "Features", Description = "The name this sends to gossip to represent itself.")]
        [UIHint("TagContainer")]
        public HashSet<string> SupportedFeatures { get; set; }

        [Display(Name = "Channels", Description = "The name this sends to gossip to represent itself.")]
        [UIHint("TagContainer")]
        public HashSet<string> SupportedChannels { get; set; }

        [Display(Name = "Retry Loop Maximum", Description = "The maximum retry value. Higher = more retries.")]
        [Range(200, 1000, ErrorMessage = "Must be between 200 and 1000.")]
        [DataType(DataType.Text)]
        public double SuspendMultiplierMaximum { get; set; }

        [Display(Name = "Retry Loop Multiplier", Description = "How much the retry loop monitor escalates each loop. Higher = less retries.")]
        [Range(1.15, 3, ErrorMessage = "Must be between 1.15 and 3.")]
        [DataType(DataType.Text)]
        public double SuspendMultiplier { get; set; }

        [Display(Name = "Backup Name", Description = "Include a name for this backup to make it a permenant archival point.")]
        [DataType(DataType.Text)]
        public string BackupName { get; set; }

        public IGossipConfig GossipConfigDataObject { get; set; }
        public IEnumerable<ILanguage> ValidLanguages { get; set; }
        public IEnumerable<IZoneTemplate> ValidZones { get; set; }
        public IGlobalConfig ConfigDataObject { get; set; }
    }
}