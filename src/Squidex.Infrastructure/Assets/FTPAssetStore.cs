// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Assets
{
    public sealed class FTPAssetStore : IAssetStore, IInitializable
    {
        private readonly string path;
        private readonly ISemanticLog log;
        private readonly string ftpMessage = "The system cannot find the file specified";

        private readonly Func<IFtpClient> createFtpClient;

        public FTPAssetStore(Func<IFtpClient> createFtpClient, string path, ISemanticLog log)
        {
            this.createFtpClient = createFtpClient;
            this.path = path;
            this.log = log;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            using (var client = createFtpClient())
            {
                await client.ConnectAsync(ct);
                if (!await client.DirectoryExistsAsync(path, ct))
                {
                    await client.CreateDirectoryAsync(path, ct);
                }
            }

            log.LogInformation(w => w
                  .WriteProperty("action", "FTPAssetStoreConfigured")
                  .WriteProperty("path", path));
        }

        public async Task CopyAsync(string sourceFileName, string targetFileName, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(sourceFileName, nameof(sourceFileName));
            Guard.NotNullOrEmpty(targetFileName, nameof(targetFileName));

            using (var client = GetFtpClient(path))
            {
                using (var stream = new MemoryStream())
                {
                    await DownloadAsync(sourceFileName, stream, ct);
                    await UploadAsync(targetFileName, stream, false, ct);
                }
            }
        }

        public async Task DeleteAsync(string fileName)
        {
            using (var client = GetFtpClient(path))
            {
                await client.DeleteFileAsync(fileName);
            }
        }

        public async Task DownloadAsync(string fileName, Stream stream, CancellationToken ct = default)
        {
            try
            {
                using (var client = GetFtpClient(path))
                {
                    await client.DownloadAsync(stream, fileName, token: ct);
                }
            }
            catch (FtpException ex) when (ex.InnerException.Message.Contains(ftpMessage))
            {
                throw new AssetNotFoundException(fileName, ex);
            }
        }

        public string GeneratePublicUrl(string fileName)
        {
            return null;
        }

        public async Task UploadAsync(string fileName, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            using (var client = GetFtpClient(path))
            {
                if (!overwrite && await client.FileExistsAsync(fileName, ct))
                {
                    throw new AssetAlreadyExistsException(fileName);
                }

                await client.UploadAsync(stream, fileName, overwrite ? FtpExists.Overwrite : FtpExists.Skip, true, null, ct);
            }
        }

        private IFtpClient GetFtpClient(string dir)
        {
            var client = createFtpClient();
            client.SetWorkingDirectory(dir);
            return client;
        }
    }
}
