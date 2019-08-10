using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Services;

namespace MediaBrowser.Controller.Net
{
    public interface ISessionContext
    {
        SessionInfo GetSession(IRequest requestContext);
        User GetUser(IRequest requestContext);
    }
}
