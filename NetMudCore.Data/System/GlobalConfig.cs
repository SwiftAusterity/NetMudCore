using NetMudCore.Data.Architectural;
using NetMudCore.Data.Architectural.PropertyBinding;
using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Administrative;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.System;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace NetMudCore.Data.System
{
    [Serializable]
    public class GlobalConfig : ConfigData, IGlobalConfig
    {

        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.None;

        /// <summary>
        /// Type of configuation data this is
        /// </summary>

        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.GameWorld;

        /// <summary>
        /// Is the websockets portal allowing new connections
        /// </summary>
        [Display(Name = "Websocket Portal Available", Description = "Are new connections being accepted over websockets?")]
        [UIHint("Boolean")]
        public bool WebsocketPortalActive { get; set; }

        /// <summary>
        /// Are new users allowed to register
        /// </summary>
        [Display(Name = "User Creation", Description = "Are new users allowed to register?")]
        [UIHint("Boolean")]
        public bool UserCreationActive { get; set; }

        /// <summary>
        /// Are only admins allowed to log in - noone at StaffRank.Player
        /// </summary>
        [Display(Name = "Admins Only", Description = "Are only admins allowed to log in - noone at StaffRank.Player?")]
        [UIHint("Boolean")]
        public bool AdminsOnly { get; set; }

        /// <summary>
        /// The API key for your azure translation service
        /// </summary>
        [Display(Name = "System Email", Description = "The email address we send email from.")]
        [DataType(DataType.Text)]
        public string SystemEmail { get; set; }

        /// <summary>
        /// The API key for your azure translation service
        /// </summary>
        [Display(Name = "System mail password", Description = "Credentials for sending system email.")]
        [DataType(DataType.Text)]
        public string SystemMailPassword { get; set; }

        /// <summary>
        /// Is live translation active?
        /// </summary>
        [Display(Name = "Live Translation", Description = "Do new Dictata get translated to the UI languages?")]
        [UIHint("Boolean")]
        public bool TranslationActive { get; set; }

        /// <summary>
        /// The API key for your azure translation service
        /// </summary>
        [Display(Name = "Azure API Key", Description = "The API key for your azure translation service.")]
        [DataType(DataType.Text)]
        public string AzureTranslationKey { get; set; }

        /// <summary>
        /// Is the deep lex active?
        /// </summary>
        [Display(Name = "Deep Lex", Description = "Do words get deep lexed through Mirriam Webster?")]
        [UIHint("Boolean")]
        public bool DeepLexActive { get; set; }

        /// <summary>
        /// Dictionary key for the deep lex
        /// </summary>
        [Display(Name = "Mirriam Dictionary Key", Description = "The API key for your mirriam webster dictionary service.")]
        [DataType(DataType.Text)]
        public string MirriamDictionaryKey { get; set; }

        /// <summary>
        /// Thesaurus key for the deep lex
        /// </summary>
        [Display(Name = "Mirriam Thesaurus Key", Description = "The API key for your mirriam webster thesaurus service.")]
        [DataType(DataType.Text)]
        public string MirriamThesaurusKey { get; set; }

        /// <summary>
        /// The base language for the system
        /// </summary>
        [JsonPropertyName("BaseLanguage")]
        private ConfigDataCacheKey BaseLanguageKey { get; set; }

        /// <summary>
        /// The language this is derived from
        /// </summary>

        [JsonIgnore]
        [Display(Name = "Base Language", Description = "The base language for the system.")]
        [UIHint("LanguageList")]
        [LanguageDataBinder]
        public ILanguage BaseLanguage
        {
            get
            {
                if (BaseLanguageKey == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<ILanguage>(BaseLanguageKey);
            }
            set
            {
                if (value == null)
                {
                    BaseLanguageKey = null;
                    return;
                }

                BaseLanguageKey = new ConfigDataCacheKey(value);
            }
        }

        public GlobalConfig()
        {
            Name = "LiveSettings";
            WebsocketPortalActive = true;
            AdminsOnly = false;
            UserCreationActive = true;

            BaseLanguage = ConfigDataCache.GetAll<ILanguage>().FirstOrDefault();
            AzureTranslationKey = string.Empty;
            MirriamDictionaryKey = string.Empty;
            MirriamThesaurusKey = string.Empty;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new GlobalConfig
            {
                Name = Name,
                WebsocketPortalActive = WebsocketPortalActive,
                UserCreationActive = UserCreationActive,
                AdminsOnly = AdminsOnly,
            };
        }
    }
}
