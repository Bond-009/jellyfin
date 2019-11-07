namespace MediaBrowser.Model.Dlna
{
    /// <summary>
    /// The delivery method for subtitles.
    /// </summary>
    public enum SubtitleDeliveryMethod
    {
        /// <summary>
        /// The encode
        /// </summary>
        Encode = 0,

        /// <summary>
        /// The embed
        /// </summary>
        Embed = 1,

        /// <summary>
        /// The external
        /// </summary>
        External = 2,

        /// <summary>
        /// The HLS
        /// </summary>
        Hls = 3
    }
}
