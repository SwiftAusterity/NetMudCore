namespace NetMudCore.Models
{
    /// <summary>
    /// Partial view model for rendering ascii maps
    /// </summary>
    public class LiveAsciiMapViewModel(string mapRenderType, string birthMark, int zIndex, int radius = -1)
    {
        /// <summary>
        /// The render type
        /// RenderRoomForEditWithRadius, RenderWorldMap, RenderZoneMap
        /// </summary>
        public string MapRenderType { get; set; } = mapRenderType;

        /// <summary>
        /// The Birthmark of the thing we're rendering
        /// </summary>
        public string BirthMark { get; set; } = birthMark;

        /// <summary>
        /// The zindex we're rendering
        /// </summary>
        public int ZIndex { get; set; } = zIndex;

        /// <summary>
        /// Radius we're rendering, only relevant to rooms right now
        /// </summary>
        public int Radius { get; set; } = radius;
    }
}