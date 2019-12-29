using System;
using System.IO;
using System.Linq;
using Emby.Naming.Common;

namespace Emby.Naming.Video
{
    public static class StubResolver
    {
        public static StubResult ResolveFile(string path, NamingOptions options)
        {
            if (path == null)
            {
                return default;
            }

            var extension = Path.GetExtension(path);
            if (!options.StubFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return default;
            }

            var result = new StubResult()
            {
                IsStub = true
            };

            ReadOnlySpan<char> path2 = Path.GetFileNameWithoutExtension(path.AsSpan());
            ReadOnlySpan<char> token = Path.GetExtension(path2).TrimStart('.');
            foreach (var rule in options.StubTypes)
            {
                if (token.Equals(rule.Token, StringComparison.OrdinalIgnoreCase))
                {
                    result.StubType = rule.StubType;
                    break;
                }
            }

            return result;
        }
    }
}
