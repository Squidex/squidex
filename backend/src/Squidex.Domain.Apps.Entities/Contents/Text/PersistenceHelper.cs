// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public static class PersistenceHelper
    {
        private const string ArchiveFile = "Archive.zip";
        private const string LockFile = "write.lock";

        public static async Task UploadDirectoryAsync(this IAssetStore assetStore, DirectoryInfo directory, IndexCommit commit)
        {
            using (var fileStream = new FileStream(
                   Path.Combine(directory.FullName, ArchiveFile),
                   FileMode.Create,
                   FileAccess.ReadWrite,
                   FileShare.None,
                   4096,
                   FileOptions.DeleteOnClose))
            {
                using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    foreach (var fileName in commit.FileNames)
                    {
                        var file = new FileInfo(Path.Combine(directory.FullName, fileName));

                        try
                        {
                            if (!file.Name.Equals(ArchiveFile, StringComparison.OrdinalIgnoreCase) &&
                                !file.Name.Equals(LockFile, StringComparison.OrdinalIgnoreCase))
                            {
                                zipArchive.CreateEntryFromFile(file.FullName, file.Name);
                            }
                        }
                        catch (IOException)
                        {
                            continue;
                        }
                    }
                }

                fileStream.Position = 0;

                await assetStore.UploadAsync(directory.Name, 0, string.Empty, fileStream, true);
            }
        }

        public static async Task DownloadAsync(this IAssetStore assetStore, DirectoryInfo directory)
        {
            if (directory.Exists)
            {
                directory.Delete(true);
            }

            directory.Create();

            using (var fileStream = new FileStream(
                   Path.Combine(directory.FullName, ArchiveFile),
                   FileMode.Create,
                   FileAccess.ReadWrite,
                   FileShare.None,
                   4096,
                   FileOptions.DeleteOnClose))
            {
                try
                {
                    await assetStore.DownloadAsync(directory.Name, 0, string.Empty, fileStream);

                    fileStream.Position = 0;

                    using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read, true))
                    {
                        zipArchive.ExtractToDirectory(directory.FullName);
                    }
                }
                catch (AssetNotFoundException)
                {
                    return;
                }
            }
        }
    }
}
