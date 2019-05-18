// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentFTP;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Assets
{
    public sealed class FTPAssetStoreFixture
    {
        private Dictionary<string, Stream> files = new Dictionary<string, Stream>();

        public string Source
        {
            get => "/file1";
        }

        public string Target
        {
            get => "/file2";
        }

        public string Cannotfindfile
        {
            get => "/Cannotfindfile";
        }

        public string FtpMessage
        {
            get => "The system cannot find the file specified";
        }

        public FTPAssetStore AssetStore { get; }
        public IFtpClient FtpClient { get; private set; }
        public ISemanticLog Log { get; private set; }

        public FTPAssetStoreFixture()
        {
            FtpClient = A.Fake<IFtpClient>();
            Log = A.Fake<ISemanticLog>();
            AssetStore = new FTPAssetStore(() => FtpClient, "/", Log);

            A.CallTo(() => FtpClient.DownloadAsync(A<Stream>.Ignored, A<string>.Ignored, A<long>.Ignored, A<IProgress<FtpProgress>>.Ignored, A<CancellationToken>.Ignored))
                 .WhenArgumentsMatch(call =>
                 {
                     string t1 = call.Get<string>(1);
                     return files.ContainsKey(t1);
                 })
                .ReturnsLazily(call =>
                {
                    string t1 = call.GetArgument<string>(1);
                    Stream t0 = call.GetArgument<Stream>(0);
                    files[t1].CopyTo(t0);
                    return Task.FromResult(true);
                });

            A.CallTo(() => FtpClient.DownloadAsync(A<Stream>.Ignored, A<string>.Ignored, A<long>.Ignored, A<IProgress<FtpProgress>>.Ignored, A<CancellationToken>.Ignored))
                .WhenArgumentsMatch(call =>
                {
                    string t1 = call.Get<string>(1);
                    return !files.ContainsKey(t1);
                })
                .Throws(new FtpException(FtpMessage, new Exception(FtpMessage)));

            A.CallTo(() => FtpClient.UploadAsync(A<Stream>.Ignored, A<string>.Ignored, A<FtpExists>._, A<bool>._, A<IProgress<FtpProgress>>._, A<CancellationToken>.Ignored))
                 .WhenArgumentsMatch(call =>
                 {
                     string t1 = call.Get<string>(1);
                     FtpExists ftpExists = call.Get<FtpExists>(2);
                     return ftpExists == FtpExists.Overwrite || !files.ContainsKey(t1);
                 })
                 .ReturnsLazily(call =>
                 {
                     Stream t1 = call.GetArgument<Stream>(0);
                     string t2 = call.GetArgument<string>(1);
                     if (!files.ContainsKey(t2))
                     {
                         files.Add(t2, t1);
                     }
                     else
                     {
                         files[t2] = t1;
                     }
                     return Task.FromResult(true);
                 });

            A.CallTo(() => FtpClient.FileExistsAsync(A<string>.Ignored, A<CancellationToken>.Ignored)).ReturnsLazily(call =>
              {
                  string t1 = call.GetArgument<string>(0);
                  return Task.FromResult(files.ContainsKey(t1));
              });

            AssetStore.UploadAsync(Source, A.Fake<Stream>()).Wait();
        }
    }
}
