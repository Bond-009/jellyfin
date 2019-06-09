using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Emby.Server.Implementations.EntryPoints
{
    public class RecordingNotifier : IServerEntryPoint
    {
        private readonly ILiveTvManager _liveTvManager;
        private readonly ISessionManager _sessionManager;
        private readonly IUserManager _userManager;
        private readonly ILogger _logger;

        public RecordingNotifier(ISessionManager sessionManager, IUserManager userManager, ILogger logger, ILiveTvManager liveTvManager)
        {
            _sessionManager = sessionManager;
            _userManager = userManager;
            _logger = logger;
            _liveTvManager = liveTvManager;
        }

        public Task RunAsync()
        {
            _liveTvManager.TimerCancelled += _liveTvManager_TimerCancelled;
            _liveTvManager.SeriesTimerCancelled += _liveTvManager_SeriesTimerCancelled;
            _liveTvManager.TimerCreated += _liveTvManager_TimerCreated;
            _liveTvManager.SeriesTimerCreated += _liveTvManager_SeriesTimerCreated;

            return Task.CompletedTask;
        }

        private void _liveTvManager_SeriesTimerCreated(object sender, MediaBrowser.Model.Events.GenericEventArgs<TimerEventInfo> e)
        {
            SendMessage("SeriesTimerCreated", e.Argument);
        }

        private void _liveTvManager_TimerCreated(object sender, MediaBrowser.Model.Events.GenericEventArgs<TimerEventInfo> e)
        {
            SendMessage("TimerCreated", e.Argument);
        }

        private void _liveTvManager_SeriesTimerCancelled(object sender, MediaBrowser.Model.Events.GenericEventArgs<TimerEventInfo> e)
        {
            SendMessage("SeriesTimerCancelled", e.Argument);
        }

        private void _liveTvManager_TimerCancelled(object sender, MediaBrowser.Model.Events.GenericEventArgs<TimerEventInfo> e)
        {
            SendMessage("TimerCancelled", e.Argument);
        }

        private async void SendMessage(string name, TimerEventInfo info)
        {
            var users = _userManager.GetUsers().Where(i => i.Policy.EnableLiveTvAccess).Select(i => i.Id).ToList();

            try
            {
                await _sessionManager.SendMessageToUserSessions(users, name, info, CancellationToken.None);
            }
            catch (ObjectDisposedException)
            {
                // TODO Log exception or Investigate and properly fix.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
            }
        }

        public void Dispose()
        {
            _liveTvManager.TimerCancelled -= _liveTvManager_TimerCancelled;
            _liveTvManager.SeriesTimerCancelled -= _liveTvManager_SeriesTimerCancelled;
            _liveTvManager.TimerCreated -= _liveTvManager_TimerCreated;
            _liveTvManager.SeriesTimerCreated -= _liveTvManager_SeriesTimerCreated;
        }
    }
}
