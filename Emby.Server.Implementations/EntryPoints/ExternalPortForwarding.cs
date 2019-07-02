using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Events;
using Microsoft.Extensions.Logging;
using Mono.Nat;

namespace Emby.Server.Implementations.EntryPoints
{
    public class ExternalPortForwarding : IServerEntryPoint
    {
        private readonly IServerApplicationHost _appHost;
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IServerConfigurationManager _config;
        private readonly IDeviceDiscovery _deviceDiscovery;

        private Timer _timer;

        private NatManager _natManager;

        private string _lastConfigIdentifier;

        private List<string> _createdRules = new List<string>();
        private List<string> _usnsHandled = new List<string>();

        private bool _disposed = false;

        public ExternalPortForwarding(
            ILogger<ExternalPortForwarding> logger,
            IServerApplicationHost appHost,
            IServerConfigurationManager config,
            IDeviceDiscovery deviceDiscovery,
            IHttpClient httpClient)
        {
            _logger = logger;
            _appHost = appHost;
            _config = config;
            _deviceDiscovery = deviceDiscovery;
            _httpClient = httpClient;
            _config.ConfigurationUpdated += OnConfigurationUpdated;
        }

        private string GetConfigIdentifier()
        {
            var config = _config.Configuration;
            StringBuilder str = new StringBuilder();

            str.Append(config.EnableUPnP);
            str.Append('|');
            str.Append(config.PublicPort);
            str.Append('|');
            str.Append(_appHost.HttpPort);
            str.Append('|');
            str.Append(_appHost.HttpsPort);
            str.Append('|');
            str.Append(_appHost.EnableHttps);
            str.Append('|');
            str.Append(config.EnableRemoteAccess);

            return str.ToString();
        }

        private async void OnConfigurationUpdated(object sender, EventArgs e)
        {
            if (!string.Equals(_lastConfigIdentifier, GetConfigIdentifier(), StringComparison.OrdinalIgnoreCase))
            {
                DisposeNat();

                await RunAsync();
            }
        }

        /// <inheritdoc />
        public Task RunAsync()
        {
            if (_config.Configuration.EnableUPnP && _config.Configuration.EnableRemoteAccess)
            {
                Start();
            }

            _config.ConfigurationUpdated -= OnConfigurationUpdated;
            _config.ConfigurationUpdated += OnConfigurationUpdated;

            return Task.CompletedTask;
        }

        private void Start()
        {
            _logger.LogDebug("Starting NAT discovery");
            if (_natManager == null)
            {
                _natManager = new NatManager(_logger, _httpClient);
                _natManager.DeviceFound += OnNatUtilityDeviceFound;
                _natManager.StartDiscovery();
            }

            _timer = new Timer(ClearCreatedRules, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

            _deviceDiscovery.DeviceDiscovered += OnDeviceDiscovered;

            _lastConfigIdentifier = GetConfigIdentifier();
        }

        private async void OnDeviceDiscovered(object sender, GenericEventArgs<UpnpDeviceInfo> e)
        {
            if (_disposed)
            {
                return;
            }

            var info = e.Argument;

            if (!info.Headers.TryGetValue("USN", out string usn))
            {
                usn = string.Empty;
            }

            if (!info.Headers.TryGetValue("NT", out string nt))
            {
                nt = string.Empty;
            }

            // Filter device type
            if (usn.IndexOf("WANIPConnection:", StringComparison.OrdinalIgnoreCase) == -1
                && nt.IndexOf("WANIPConnection:", StringComparison.OrdinalIgnoreCase) == -1
                && usn.IndexOf("WANPPPConnection:", StringComparison.OrdinalIgnoreCase) == -1
                && nt.IndexOf("WANPPPConnection:", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return;
            }

            var identifier = string.IsNullOrWhiteSpace(usn) ? nt : usn;

            if (info.Location == null)
            {
                return;
            }

            lock (_usnsHandled)
            {
                if (_usnsHandled.Contains(identifier))
                {
                    return;
                }

                _usnsHandled.Add(identifier);
            }

            _logger.LogDebug("Found NAT device: {Id}", identifier);

            if (IPAddress.TryParse(info.Location.Host, out var address))
            {

                string localAddressString;

                try
                {
                    localAddressString = await _appHost.GetLocalApiUrl(CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error");
                    return;
                }

                if (Uri.TryCreate(localAddressString, UriKind.Absolute, out Uri uri))
                {
                    localAddressString = uri.Host;
                }

                if (!IPAddress.TryParse(localAddressString, out IPAddress localAddress))
                {
                    return;
                }

                if (_disposed)
                {
                    return;
                }

                if (_natManager != null)
                {
                    // The Handle method doesn't need the port
                    var endpoint = new IPEndPoint(address, info.Location.Port);
                    await _natManager.Handle(localAddress, info, endpoint, NatProtocol.Upnp).ConfigureAwait(false);
                }
            }
        }

        private void ClearCreatedRules(object state)
        {
            lock (_createdRules)
            {
                _createdRules.Clear();
            }

            lock (_usnsHandled)
            {
                _usnsHandled.Clear();
            }
        }

        private async void OnNatUtilityDeviceFound(object sender, DeviceEventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                var device = e.Device;

                await CreateRules(device).ConfigureAwait(false);
            }
            catch
            {
                // Commenting out because users are reporting problems out of our control
                //_logger.LogError(ex, "Error creating port forwarding rules");
            }
        }

        private async Task CreateRules(INatDevice device)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            // On some systems the device discovered event seems to fire repeatedly
            // This check will help ensure we're not trying to port map the same device over and over
            var address = device.LocalAddress;

            var addressString = address.ToString();

            lock (_createdRules)
            {
                if (!_createdRules.Contains(addressString))
                {
                    _createdRules.Add(addressString);
                }
                else
                {
                    return;
                }
            }

            try
            {
                await CreatePortMap(device, _appHost.HttpPort, _config.Configuration.PublicPort).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating http port map");
                return;
            }

            try
            {
                await CreatePortMap(device, _appHost.HttpsPort, _config.Configuration.PublicHttpsPort).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating https port map");
            }
        }

        private Task CreatePortMap(INatDevice device, int privatePort, int publicPort)
        {
            _logger.LogDebug("Creating port map on local port {0} to public port {1} with device {2}", privatePort, publicPort, device.LocalAddress.ToString());

            return device.CreatePortMap(new Mapping(Protocol.Tcp, privatePort, publicPort)
            {
                Description = _appHost.Name
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _disposed = true;
            DisposeNat();
        }

        private void DisposeNat()
        {
            _logger.LogDebug("Stopping NAT discovery");

            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            _deviceDiscovery.DeviceDiscovered -= OnDeviceDiscovered;

            var natManager = _natManager;

            if (natManager != null)
            {
                _natManager = null;

                using (natManager)
                {
                    try
                    {
                        natManager.StopDiscovery();
                        natManager.DeviceFound -= OnNatUtilityDeviceFound;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error stopping NAT Discovery");
                    }
                }
            }
        }
    }
}
