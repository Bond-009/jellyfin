using System;
using System.Runtime.InteropServices;
using System.Threading;
using MediaBrowser.Model.System;

namespace MediaBrowser.Common.System
{
    /// <summary>
    /// Identifies the operating system.
    /// </summary>
    public static class OperatingSystem
    {
        // We can't use Interlocked.CompareExchange for enums
        private static int _id = int.MaxValue;

        /// <summary>
        /// Gets the operating system ID.
        /// </summary>
        /// <value>The operating system ID.</value>
        public static OperatingSystemId Id
        {
            get
            {
                if (_id == int.MaxValue)
                {
                    Interlocked.CompareExchange(ref _id, (int)GetId(), int.MaxValue);
                }

                return (OperatingSystemId)_id;
            }
        }

        /// <summary>
        /// Gets the user-friendly name for the operating system.
        /// </summary>
        /// <value>A user-friendly name of the operating system.</value>
        public static string Name
        {
            get
            {
                switch (Id)
                {
                    case OperatingSystemId.BSD: return "BSD";
                    case OperatingSystemId.Linux: return "Linux";
                    case OperatingSystemId.Darwin: return "macOS";
                    case OperatingSystemId.Windows: return "Windows";
                    default: throw new PlatformNotSupportedException($"Unknown OS {Id}");
                }
            }
        }

        private static OperatingSystemId GetId()
        {
            switch (Environment.OSVersion.Platform)
            {
                // On .NET Core `MacOSX` got replaced by `Unix`, this case should never be hit.
                case PlatformID.MacOSX:
                    return OperatingSystemId.Darwin;
                case PlatformID.Win32NT:
                    return OperatingSystemId.Windows;
                case PlatformID.Unix:
                default:
                    {
                        string osDescription = RuntimeInformation.OSDescription;
                        if (osDescription.IndexOf("linux", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            return OperatingSystemId.Linux;
                        }
                        else if (osDescription.IndexOf("darwin", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            return OperatingSystemId.Darwin;
                        }
                        else if (osDescription.IndexOf("bsd", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            return OperatingSystemId.BSD;
                        }

                        throw new PlatformNotSupportedException($"Can't resolve OS with description: '{osDescription}'");
                    }
            }
        }
    }
}
