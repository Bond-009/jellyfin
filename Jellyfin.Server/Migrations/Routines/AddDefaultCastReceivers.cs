using System;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.System;

namespace Jellyfin.Server.Migrations.Routines;

/// <summary>
/// Migration to add the default cast receivers to the system config.
/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
[JellyfinMigration("2025-04-20T16:00:00", nameof(AddDefaultCastReceivers), "34A1A1C4-5572-418E-A2F8-32CDFE2668E8", RunMigrationOnSetup = true)]
public class AddDefaultCastReceivers : IMigrationRoutine
#pragma warning restore CS0618 // Type or member is obsolete
{
    private readonly IServerConfigurationManager _serverConfigurationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddDefaultCastReceivers"/> class.
    /// </summary>
    /// <param name="serverConfigurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
    public AddDefaultCastReceivers(IServerConfigurationManager serverConfigurationManager)
    {
        _serverConfigurationManager = serverConfigurationManager;
    }

    /// <inheritdoc />
    public void Perform()
    {
        _serverConfigurationManager.Configuration.CastReceiverApplications =
        [
            new()
            {
                Id = "F007D354",
                Name = "Stable"
            },
            new()
            {
                Id = "6F511C87",
                Name = "Unstable"
            }
        ];

        _serverConfigurationManager.SaveConfiguration();
    }
}
