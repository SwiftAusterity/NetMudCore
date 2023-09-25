﻿namespace NetMudCore.DataStructure.Linguistic
{
    /// <summary>
    /// Word/phrase types
    /// </summary>
    public enum SentenceType : short
    {
        None = -1,
        Partial = 0,
        Statement = 1,
        Question = 2,
        Exclamation = 3,
        ExclamitoryQuestion = 4
    }
}
