using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jellyfin.Extensions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;

namespace MediaBrowser.LocalMetadata.Images
{
    /// <summary>
    /// Episode local image provider.
    /// </summary>
    public class EpisodeLocalImageProvider : ILocalImageProvider, IHasOrder
    {
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="EpisodeLocalImageProvider"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        public EpisodeLocalImageProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <inheritdoc />
        public string Name => "Local Images";

        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        public bool Supports(BaseItem item)
        {
            return item is Episode && item.SupportsLocalMetadata;
        }

        /// <inheritdoc />
        public IEnumerable<LocalImageInfo> GetImages(BaseItem item)
        {
            var parentPath = Path.GetDirectoryName(item.Path);
            if (parentPath is null)
            {
                return Enumerable.Empty<LocalImageInfo>();
            }

            var parentPathFiles = _fileSystem.GetFiles(parentPath);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(item.Path.AsSpan()).ToString();

            var images = GetImageFilesFromFolder(nameWithoutExtension, parentPathFiles);

            var metadataSubDir = _fileSystem.GetDirectories(parentPath).FirstOrDefault(d => d.Name.Equals("metadata", StringComparison.Ordinal));
            if (metadataSubDir is not null)
            {
                var files = _fileSystem.GetFiles(metadataSubDir.FullName);
                images.AddRange(GetImageFilesFromFolder(nameWithoutExtension, files));
            }

            return images;
        }

        private List<LocalImageInfo> GetImageFilesFromFolder(ReadOnlySpan<char> filenameWithoutExtension, IEnumerable<FileSystemMetadata> filePaths)
        {
            var list = new List<LocalImageInfo>(1);
            var thumbName = string.Concat(filenameWithoutExtension, "-thumb");

            foreach (var i in filePaths)
            {
                if (i.IsDirectory)
                {
                    continue;
                }

                if (BaseItem.SupportedImageExtensions.Contains(i.Extension.AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    var currentNameWithoutExtension = Path.GetFileNameWithoutExtension(i.FullName.AsSpan());

                    if (filenameWithoutExtension.Equals(currentNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(new LocalImageInfo { FileInfo = i, Type = ImageType.Primary });
                    }
                    else if (currentNameWithoutExtension.Equals(thumbName, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(new LocalImageInfo { FileInfo = i, Type = ImageType.Primary });
                    }
                }
            }

            return list;
        }
    }
}
