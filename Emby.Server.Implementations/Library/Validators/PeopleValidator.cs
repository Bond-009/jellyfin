using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace Emby.Server.Implementations.Library.Validators
{
    /// <summary>
    /// Class PeopleValidator.
    /// </summary>
    public class PeopleValidator
    {
        /// <summary>
        /// The _library manager.
        /// </summary>
        private readonly ILibraryManager _libraryManager;

        /// <summary>
        /// The _logger.
        /// </summary>
        private readonly ILogger _logger;

        private readonly IDirectoryService _directoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeopleValidator" /> class.
        /// </summary>
        /// <param name="libraryManager">The library manager.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="directoryService">The directory service.</param>
        public PeopleValidator(ILibraryManager libraryManager, ILogger logger, IDirectoryService directoryService)
        {
            _libraryManager = libraryManager;
            _logger = logger;
            _directoryService = directoryService;
        }

        /// <summary>
        /// Validates the people.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>Task.</returns>
        public async Task ValidatePeople(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var people = _libraryManager.GetPeopleNames(new InternalPeopleQuery());

            var numComplete = 0;

            var numPeople = people.Count;

            _logger.LogDebug("Will refresh {0} people", numPeople);

            foreach (var person in people)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var item = _libraryManager.GetPerson(person);

                    var options = new MetadataRefreshOptions(_directoryService)
                    {
                        ImageRefreshMode = MetadataRefreshMode.ValidationOnly,
                        MetadataRefreshMode = MetadataRefreshMode.ValidationOnly
                    };

                    await item.RefreshMetadata(options, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating IBN entry {Person}", person);
                }

                // Update progress
                numComplete++;
                double percent = numComplete;
                percent /= numPeople;

                progress.Report(100 * percent);
            }

            var deadEntities = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Person },
                IsDeadPerson = true,
                IsLocked = false
            });

            foreach (var item in deadEntities)
            {
                _logger.LogInformation(
                    "Deleting dead {2} {0} {1}.",
                    item.Id.ToString("N", CultureInfo.InvariantCulture),
                    item.Name,
                    item.GetType().Name);

                _libraryManager.DeleteItem(
                    item,
                    new DeleteOptions
                    {
                        DeleteFileLocation = false
                    },
                    false);
            }

            progress.Report(100);

            _logger.LogInformation("People validation complete");
        }
    }
}
