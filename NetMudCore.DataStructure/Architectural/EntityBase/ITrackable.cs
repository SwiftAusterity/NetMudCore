﻿using NetMudCore.DataStructure.Linguistic;

namespace NetMudCore.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Indicates an entity's movement can be tracked
    /// </summary>
    public interface ITrackable
    {
        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the output</returns>
        ILexicalParagraph RenderToTrack(IEntity actor);
    }
}
