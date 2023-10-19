using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zio;

namespace MonkeyLoader
{
    /// <summary>
    /// Extensions because Zio is stupid.
    /// </summary>
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Uses EnumeratePaths of an <see cref="IFileSystem"/> to check for a directory, instead of <see cref="IFileSystem.DirectoryExists(UPath)"/>,
        /// because that doesn't work properly for zip files.
        /// </summary>
        /// <param name="fileSystem">The file system to check for a directory.</param>
        /// <param name="path">The path to the directory. Must have at least one file somewhere.</param>
        /// <returns>Whether the directory exists.</returns>
        public static bool SmartDirectoryExists(this IFileSystem fileSystem, UPath path)
            => fileSystem.EnumeratePaths("/", "*", SearchOption.AllDirectories).Any(p => p.FullName.StartsWith(path.FullName, StringComparison.OrdinalIgnoreCase));
    }
}