﻿using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Linguistic;

namespace NetMudCore.DataStructure.Inanimate
{
    /// <summary>
    /// For when something that can be held is looked at as being held
    /// </summary>
    public interface IRenderAsHeld
    {
        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="holder">entity holding the thing</param>
        /// <returns>the output</returns>
        ILexicalParagraph RenderAsHeld(IEntity viewer, IEntity holder);
    }
}
