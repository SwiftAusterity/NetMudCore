using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Linguistic;

namespace NetMudCore.DataStructure.System
{
    /// <summary>
    /// Global settings for the entire system
    /// </summary>
    public interface IGlobalConfig : IConfigData
    {
        /// <summary>
        /// Is the websockets portal allowing new connections
        /// </summary>
        bool WebsocketPortalActive { get; set; }

        /// <summary>
        /// Are new users allowed to register
        /// </summary>
        bool UserCreationActive { get; set; }

        /// <summary>
        /// Are only admins allowed to log in - noone at StaffRank.Player
        /// </summary>
        bool AdminsOnly { get; set; }

        /// <summary>
        /// What email do we send from?
        /// </summary>
        string SystemEmail { get; set; }

        /// <summary>
        /// Credentials for mailing
        /// </summary>
        string SystemMailPassword { get; set; }

        /// <summary>
        /// Is live translation active?
        /// </summary>
        bool TranslationActive { get; set; }

        /// <summary>
        /// The API key for your azure translation service
        /// </summary>
        string AzureTranslationKey { get; set; }

        /// <summary>
        /// Is the deep lex active?
        /// </summary>
        bool DeepLexActive { get; set; }

        /// <summary>
        /// Dictionary key for the deep lex
        /// </summary>
        string MirriamDictionaryKey { get; set; }

        /// <summary>
        /// Thesaurus key for the deep lex
        /// </summary>
        string MirriamThesaurusKey { get; set; }

        /// <summary>
        /// The base language for the system
        /// </summary>
        ILanguage BaseLanguage { get; set; }
    }
}
