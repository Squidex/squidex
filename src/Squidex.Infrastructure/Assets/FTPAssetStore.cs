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
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Assets
{
    public sealed class FTPAssetStore : IAssetStore, IInitializable
    {
        private readonly string path;
        private readonly ISemanticLog log;
        private readonly Func<IFtpClient> factory;

        public FTPAssetStore(Func<IFtpClient> factory, string path, ISemanticLog log)
        {
            Guard.NotNull(factory, nameof(factory));
            Guard.NotNullOrEmpty(path, nameof(path));
            Guard.NotNull(log, nameof(log));

            this.factory = factory;
            this.path = path;

            this.log = log;
        }

        public string GeneratePublicUrl(string fileName)
        {
            return null;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            using (var client = factory())
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

            using (var client = GetFtpClient())
            {
                var tempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose))
                {
                    await DownloadAsync(client, sourceFileName, stream, ct);
                    await UploadAsync(client, targetFileName, stream, false, ct);
                }
            }
        }

        public async Task DownloadAsync(string fileName, Stream stream, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));
            Guard.NotNull(stream, nameof(stream));

            using (var client = GetFtpClient())
            {
                await DownloadAsync(client, fileName, stream, ct);
            }
        }

        public async Task UploadAsync(string fileName, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));
            Guard.NotNull(stream, nameof(stream));

            using (var client = GetFtpClient())
            {
                await UploadAsync(client, fileName, stream, overwrite, ct);
            }
        }

        private static async Task DownloadAsync(IFtpClient client, string fileName, Stream stream, CancellationToken ct)
        {
            try
            {
                await client.DownloadAsync(stream, fileName, token: ct);
            }
            catch (FtpException ex) when (IsNotFound(ex))
            {
                throw new AssetNotFoundException(fileName, ex);
            }
        }

        private static async Task UploadAsync(IFtpClient client, string fileName, Stream stream, bool overwrite, CancellationToken ct)
        {
            if (!overwrite && await client.FileExistsAsync(fileName, ct))
            {
                throw new AssetAlreadyExistsException(fileName);
            }

            await client.UploadAsync(stream, fileName, overwrite ? FtpExists.Overwrite : FtpExists.Skip, true, null, ct);
        }

        public async Task DeleteAsync(string fileName)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));

            using (var client = GetFtpClient())
            {
                try
                {
                    await client.DeleteFileAsync(fileName);
                }
                catch (FtpException ex)
                {
                    if (!IsNotFound(ex))
                    {
                        throw ex;
                    }
                }
            }
        }

        private IFtpClient GetFtpClient()
        {
            var client = factory();

            client.Connect();
            client.SetWorkingDirectory(path);

            return client;
        }

        private static bool IsNotFound(Exception exception)
        {
            if (exception is FtpCommandException command)
            {
                return command.CompletionCode == "550";
            }

            return exception.InnerException != null && IsNotFound(exception.InnerException);
        }
    }
}