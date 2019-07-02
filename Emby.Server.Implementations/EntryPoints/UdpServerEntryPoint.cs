using System;
using System.Threading.Tasks;
using Emby.Server.Implementations.Udp;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Emby.Server.Implementations.EntryPoints
{
    /// <summary>
    /// Class UdpServerEntryPoint
    /// </summary>
    public class UdpServerEntryPoint : IServerEntryPoint
    {
        /// <summary>
        /// The port number used by the UDP server.
        /// </summary>
        public const int PortNumber = 7359;

        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogger<UdpServer> _logger;
        private readonly ISocketFactory _socketFactory;
        private readonly IServerApplicationHost _appHost;
        private readonly IJsonSerializer _json;

        /// <summary>
        /// Gets or sets the UDP server.
        /// </summary>
        /// <value>The UDP server.</value>
        private UdpServer _udpServer;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServerEntryPoint" /> class.
        /// </summary>
        public UdpServerEntryPoint(
            ILogger<UdpServer> logger,
            IServerApplicationHost appHost,
            IJsonSerializer json,
            ISocketFactory socketFactory)
        {
            _logger = logger;
            _appHost = appHost;
            _json = json;
            _socketFactory = socketFactory;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public Task RunAsync()
        {
            var _udpServer = new UdpServer(_logger, _appHost, _json, _socketFactory);

            try
            {
                _udpServer.Start(PortNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start UDP Server");
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            if (_disposed)
            {
                return;
            }

            if (dispose)
            {
                _udpServer?.Dispose();
            }

            _udpServer = null;

            _disposed = true;
        }
    }
}
