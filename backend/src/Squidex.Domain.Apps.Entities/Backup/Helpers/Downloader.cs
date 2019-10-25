﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Backup.Helpers
{
    public static class Downloader
    {
        public static async Task DownloadAsync(this IBackupArchiveLocation backupArchiveLocation, Uri url, string id)
        {
            if (string.Equals(url.Scheme, "file"))
            {
                try
                {
                    using (var targetStream = await backupArchiveLocation.OpenStreamAsync(id))
                    {
                        using (var sourceStream = new FileStream(url.LocalPath, FileMode.Open, FileAccess.Read))
                        {
                            await sourceStream.CopyToAsync(targetStream);
                        }
                    }
                }
                catch (IOException ex)
                {
                    throw new BackupRestoreException($"Cannot download the archive: {ex.Message}.", ex);
                }
            }
            else
            {
                HttpResponseMessage? response = null;
                try
                {
                    using (var client = new HttpClient())
                    {
                        response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();

                        using (var sourceStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var targetStream = await backupArchiveLocation.OpenStreamAsync(id))
                            {
                                await sourceStream.CopyToAsync(targetStream);
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    throw new BackupRestoreException($"Cannot download the archive. Got status code: {response?.StatusCode}.", ex);
                }
            }
        }

        public static async Task<BackupReader> OpenArchiveAsync(this IBackupArchiveLocation backupArchiveLocation, string id, IJsonSerializer serializer)
        {
            Stream? stream = null;

            try
            {
                stream = await backupArchiveLocation.OpenStreamAsync(id);

                return new BackupReader(serializer, stream);
            }
            catch (IOException)
            {
                stream?.Dispose();

                throw new BackupRestoreException("The backup archive is correupt and cannot be opened.");
            }
            catch (Exception)
            {
                stream?.Dispose();

                throw;
            }
        }
    }
}
