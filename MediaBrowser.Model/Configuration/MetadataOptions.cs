#pragma warning disable CS1591

using System;

namespace MediaBrowser.Model.Configuration
{
    /// <summary>
    /// Class MetadataOptions.
    /// </summary>
    public class MetadataOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataOptions"/> class.
        /// </summary>
        public MetadataOptions()
        {
            DisabledMetadataSavers = Array.Empty<string>();
            LocalMetadataReaderOrder = Array.Empty<string>();
            DisabledMetadataFetchers = Array.Empty<string>();
            MetadataFetcherOrder = Array.Empty<string>();
            DisabledImageFetchers = Array.Empty<string>();
            ImageFetcherOrder = Array.Empty<string>();
        }

        public string ItemType { get; set; }

        public string[] DisabledMetadataSavers { get; set; }

        public string[] LocalMetadataReaderOrder { get; set; }

        public string[] DisabledMetadataFetchers { get; set; }

        public string[] MetadataFetcherOrder { get; set; }

        public string[] DisabledImageFetchers { get; set; }

        public string[] ImageFetcherOrder { get; set; }

    }
}
