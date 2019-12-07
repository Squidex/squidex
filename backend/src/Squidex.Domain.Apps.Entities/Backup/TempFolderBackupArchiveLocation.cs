// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    [ExcludeFromCodeCoverage]
    public sealed class TempFolderBackupArchiveLocation : IBackupArchiveLocation
    {
        private readonly IJsonSerializer jsonSerializer;

        public TempFolderBackupArchiveLocation(IJsonSerializer jsonSerializer)
        {
            Guard.NotNull(jsonSerializer);

            this.jsonSerializer = jsonSerializer;
        }

        public async Task<IBackupReader> OpenReaderAsync(Uri url, string id)
        {
            if (string.Equals(url.Scheme, "file"))
            {
                try
                {
                    using (var targetStream = await OpenStreamAsync(id))
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
                            using (var targetStream = await OpenStreamAsync(id))
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

            Stream? stream = null;

            try
            {
                stream = await OpenStreamAsync(id);

                return new BackupReader(jsonSerializer, stream);
            }
            catch (IOException)
            {
                stream?.Dispose();

                throw new BackupRestoreException("The backup archive is corrupt and cannot be opened.");
            }
            catch (Exception)
            {
                stream?.Dispose();

                throw;
            }
        }

        public Task<Stream> OpenStreamAsync(string backupId)
        {
            var tempFile = GetTempFile(backupId);

            return Task.FromResult<Stream>(new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite));
        }

        public Task<IBackupWriter> OpenWriterAsync(Stream stream)
        {
            var writer = new BackupWriter(jsonSerializer, stream, true);

            return Task.FromResult<IBackupWriter>(writer);
        }

        public Task DeleteArchiveAsync(string backupId)
        {
            var tempFile = GetTempFile(backupId);

            try
            {
                File.Delete(tempFile);
            }
            catch (IOException)
            {
            }

            return TaskHelper.Done;
        }

        private static string GetTempFile(string backupId)
        {
            return Path.Combine(Path.GetTempPath(), backupId + ".zip");
        }
    }
}
