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

        public async Task<IBackupReader> OpenReaderAsync(Uri url, Guid id)
        {
            var stream = OpenStream(id);

            if (string.Equals(url.Scheme, "file"))
            {
                try
                {
                    using (var sourceStream = new FileStream(url.LocalPath, FileMode.Open, FileAccess.Read))
                    {
                        await sourceStream.CopyToAsync(stream);
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
                            await sourceStream.CopyToAsync(stream);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    throw new BackupRestoreException($"Cannot download the archive. Got status code: {response?.StatusCode}.", ex);
                }
            }

            try
            {
                return new BackupReader(jsonSerializer, stream);
            }
            catch (IOException)
            {
                stream.Dispose();

                throw new BackupRestoreException("The backup archive is corrupt and cannot be opened.");
            }
            catch (Exception)
            {
                stream.Dispose();

                throw;
            }
        }

        public Stream OpenStream(Guid backupId)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), backupId + ".zip");

            var fileStream = new FileStream(
                tempFile,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                4096,
                FileOptions.DeleteOnClose);

            return fileStream;
        }

        public Task<IBackupWriter> OpenWriterAsync(Stream stream)
        {
            var writer = new BackupWriter(jsonSerializer, stream, true);

            return Task.FromResult<IBackupWriter>(writer);
        }
    }
}
