// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Backup;

[ExcludeFromCodeCoverage]
public sealed class TempFolderBackupArchiveLocation : IBackupArchiveLocation
{
    private readonly IJsonSerializer serializer;
    private readonly IHttpClientFactory httpClientFactory;

    public TempFolderBackupArchiveLocation(IJsonSerializer serializer, IHttpClientFactory httpClientFactory)
    {
        this.serializer = serializer;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<IBackupReader> OpenReaderAsync(Uri url, DomainId id,
        CancellationToken ct)
    {
        Stream stream;

        if (string.Equals(url.Scheme, "file", StringComparison.OrdinalIgnoreCase))
        {
            stream = new FileStream(url.LocalPath, FileMode.Open, FileAccess.Read);
        }
        else
        {
            stream = OpenStream(id);

            HttpResponseMessage? response = null;
            try
            {
                var httpClient = httpClientFactory.CreateClient("Backup");

                response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                await using (var sourceStream = await response.Content.ReadAsStreamAsync(ct))
                {
                    await sourceStream.CopyToAsync(stream, ct);
                }
            }
            catch (HttpRequestException ex)
            {
                var statusCode = response != null ? (int)response.StatusCode : 0;

                throw new BackupRestoreException($"Cannot download the archive. Got status code {statusCode}: {ex.Message}.", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        try
        {
            return new BackupReader(serializer, stream);
        }
        catch (IOException)
        {
            await stream.DisposeAsync();

            throw new BackupRestoreException("The backup archive is corrupt and cannot be opened.");
        }
        catch (Exception)
        {
            await stream.DisposeAsync();

            throw;
        }
    }

    public Stream OpenStream(DomainId backupId)
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

    public Task<IBackupWriter> OpenWriterAsync(Stream stream,
        CancellationToken ct)
    {
        var writer = new BackupWriter(serializer, stream, true);

        return Task.FromResult<IBackupWriter>(writer);
    }
}
