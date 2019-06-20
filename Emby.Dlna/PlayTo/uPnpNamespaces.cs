using System.Xml.Linq;

namespace Emby.Dlna.PlayTo
{
    public static class uPnpNamespaces
    {
        public static readonly XNamespace dc = "http://purl.org/dc/elements/1.1/";
        public static readonly XNamespace ns = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
        public static readonly XNamespace svc = "urn:schemas-upnp-org:service-1-0";
        public static readonly XNamespace ud = "urn:schemas-upnp-org:device-1-0";
        public static readonly XNamespace upnp = "urn:schemas-upnp-org:metadata-1-0/upnp/";
        public static readonly XNamespace RenderingControl = "urn:schemas-upnp-org:service:RenderingControl:1";
        public static readonly XNamespace AvTransport = "urn:schemas-upnp-org:service:AVTransport:1";
        public static readonly XNamespace ContentDirectory = "urn:schemas-upnp-org:service:ContentDirectory:1";

        public static readonly XName containers = ns + "container";
        public static readonly XName items = ns + "item";
        public static readonly XName title = dc + "title";
        public static readonly XName creator = dc + "creator";
        public static readonly XName artist = upnp + "artist";
        public static readonly XName Id = "id";
        public static readonly XName ParentId = "parentID";
        public static readonly XName uClass = upnp + "class";
        public static readonly XName Artwork = upnp + "albumArtURI";
        public static readonly XName Description = dc + "description";
        public static readonly XName LongDescription = upnp + "longDescription";
        public static readonly XName Album = upnp + "album";
        public static readonly XName Author = upnp + "author";
        public static readonly XName Director = upnp + "director";
        public static readonly XName PlayCount = upnp + "playbackCount";
        public static readonly XName Tracknumber = upnp + "originalTrackNumber";
        public static readonly XName Res = ns + "res";
        public static readonly XName Duration = "duration";
        public static readonly XName ProtocolInfo = "protocolInfo";

        public static readonly XName ServiceStateTable = svc + "serviceStateTable";
        public static readonly XName StateVariable = svc + "stateVariable";
    }
}
