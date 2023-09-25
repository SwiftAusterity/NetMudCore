﻿using NetMudCore.DataStructure.Linguistic;

namespace NetMudCore.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Indicates an entity can be Inspected (part of rendering)
    /// </summary>
    public interface IInspectable
    {
        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        ILexicalParagraph RenderToInspect(IEntity viewer);
    }
}
