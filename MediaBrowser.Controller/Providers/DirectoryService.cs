#pragma warning disable CS1591

using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.IO;

namespace MediaBrowser.Controller.Providers;

public class DirectoryService : IDirectoryService
{
    private readonly IFileSystem _fileSystem;

    public DirectoryService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public FileSystemMetadata[] GetFileSystemEntries(string path)
        => _fileSystem.GetFileSystemEntries(path).ToArray();

    public List<FileSystemMetadata> GetDirectories(string path)
        => _fileSystem.GetDirectories(path).ToList();

    public List<FileSystemMetadata> GetFiles(string path)
        => _fileSystem.GetFiles(path).ToList();

    public FileSystemMetadata? GetFile(string path)
        => _fileSystem.GetFileInfo(path);

    public FileSystemMetadata? GetDirectory(string path)
        => _fileSystem.GetDirectoryInfo(path);

    public FileSystemMetadata? GetFileSystemEntry(string path)
        => _fileSystem.GetFileSystemInfo(path);

    public IReadOnlyList<string> GetFilePaths(string path)
        => GetFilePaths(path, false);

    public IReadOnlyList<string> GetFilePaths(string path, bool clearCache, bool sort = false)
    {
        var filePaths = _fileSystem.GetFilePaths(path);

        if (sort)
        {
            filePaths.Order();
        }

        return filePaths.ToList();
    }

    public bool IsAccessible(string path)
    {
        return _fileSystem.GetFileSystemEntryPaths(path).Any();
    }
}
