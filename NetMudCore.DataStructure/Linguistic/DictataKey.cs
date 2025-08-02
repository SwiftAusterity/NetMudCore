namespace NetMudCore.DataStructure.Linguistic
{
    /// <summary>
    /// A key for referencing dictata
    /// </summary>
    [Serializable]
    public class DictataKey(string lexemeKey, short formId)
    {
        /// <summary>
        /// the lexeme config cache key unique id
        /// </summary>
        public string LexemeKey { get; set; } = lexemeKey;

        /// <summary>
        /// The form id within the lexeme for the dictata
        /// </summary>
        public short FormId { get; set; } = formId;
    }
}
