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

namespace Squidex.Extensions.Samples.AssetStore
{
    public sealed class FTPAssetStore : IAssetStore, IInitializable
    {
        private readonly string host;
        private readonly int port;
        private readonly string username;
        private readonly string password;
        private readonly string path;

        public FTPAssetStore(string host, int port, string username, string password, string path)
        {
            this.host = host;
            this.port = port;
            this.username = username;
            this.password = password;
            this.path = path;
        }

        private FtpClient GetFtpClient()
        {
            var client = new FtpClient(host, port, username, password);
            client.Connect();
            return client;
        }

        private FtpClient GetFtpClient(string dir)
        {
            var client = GetFtpClient();
            client.SetWorkingDirectory(dir);
            return client;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            using (var client = GetFtpClient())
            {
                if (!await client.DirectoryExistsAsync(path, ct))
                {
                    await client.CreateDirectoryAsync(path, ct);
                }
            }
        }

        public async Task CopyAsync(string sourceFileName, string targetFileName, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(sourceFileName, nameof(sourceFileName));
            Guard.NotNullOrEmpty(targetFileName, nameof(targetFileName));

            using (var stream = new MemoryStream())
            {
                using (var client = GetFtpClient(path))
                {
                    await client.DownloadAsync(stream, sourceFileName, token: ct);
                    await client.UploadAsync(stream, targetFileName, createRemoteDir: true, token: ct);
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
            catch (FtpException ex)
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
                await client.UploadAsync(stream, fileName, overwrite ? FtpExists.Overwrite : FtpExists.Skip, true, null, ct);
            }
        }
    }
}
