namespace MediaBrowser.Model.Dlna
{
    /// <summary>
    /// The identifying information for a device.
    /// </summary>
    public class DeviceIdentification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceIdentification"/> class.
        /// </summary>
        public DeviceIdentification()
        {
            Headers = global::System.Array.Empty<HttpHeaderInfo>();
        }

        /// <summary>
        /// Gets or sets the name of the friendly.
        /// </summary>
        /// <value>The name of the friendly.</value>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the model number.
        /// </summary>
        /// <value>The model number.</value>
        public string ModelNumber { get; set; }

        /// <summary>
        /// Gets or sets the serial number.
        /// </summary>
        /// <value>The serial number.</value>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        /// <value>The name of the model.</value>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the model description.
        /// </summary>
        /// <value>The model description.</value>
        public string ModelDescription { get; set; }

        /// <summary>
        /// Gets or sets the device description.
        /// </summary>
        /// <value>The device description.</value>
        public string DeviceDescription { get; set; }

        /// <summary>
        /// Gets or sets the model URL.
        /// </summary>
        /// <value>The model URL.</value>
        public string ModelUrl { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer.
        /// </summary>
        /// <value>The manufacturer.</value>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer URL.
        /// </summary>
        /// <value>The manufacturer URL.</value>
        public string ManufacturerUrl { get; set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>The headers.</value>
        public HttpHeaderInfo[] Headers { get; set; }
    }
}
