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
using Squidex.ClientLibrary.Management;

namespace TestSuite.Fixtures
{
    public class AssetFixture : CreatedAppFixture
    {
        public IAssetsClient Assets { get; }

        public AssetFixture()
        {
            Assets = ClientManager.CreateAssetsClient();
        }

        public async Task<MemoryStream> DownloadAsync(AssetDto asset)
        {
            var temp = new MemoryStream();

            using (var client = new HttpClient())
            {
                var url = $"{ServerUrl}{asset._links["content"].Href}";

                var response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    await stream.CopyToAsync(temp);
                }
            }

            return temp;
        }

        public async Task<AssetDto> UploadFileAsync(string path, AssetDto asset, string fileName = null)
        {
            var fileInfo = new FileInfo(path);

            using (var stream = fileInfo.OpenRead())
            {
                var upload = new FileParameter(stream, fileName ?? RandomName(fileInfo), asset.MimeType);

                return await Assets.PutAssetContentAsync(AppName, asset.Id.ToString(), upload);
            }
        }

        public async Task<AssetDto> UploadFileAsync(string path, string mimeType, string fileName = null)
        {
            var fileInfo = new FileInfo(path);

            using (var stream = fileInfo.OpenRead())
            {
                var upload = new FileParameter(stream, fileName ?? RandomName(fileInfo), mimeType);

                return await Assets.PostAssetAsync(AppName, upload);
            }
        }

        private static string RandomName(FileInfo fileInfo)
        {
            var fileName = $"{Guid.NewGuid()}{fileInfo.Extension}";

            return fileName;
        }
    }
}
