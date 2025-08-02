namespace NetMudCore.Models
{
    /// <summary>
    /// Partial view model for rendering ascii maps
    /// </summary>
    public class AsciiMapViewModel(string mapRenderType, long dataId, int zIndex, int radius = -1)
    {
        /// <summary>
        /// The render type
        /// RenderRoomForEditWithRadius, RenderWorldMap, RenderZoneMap
        /// </summary>
        public string MapRenderType { get; set; } = mapRenderType;

        /// <summary>
        /// The Id of the thing we're rendering
        /// </summary>
        public long DataID { get; set; } = dataId;

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