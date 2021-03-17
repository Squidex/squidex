// ==========================================================================
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
        public IAssetsClient Assets => Squidex.Assets;

        public async Task<MemoryStream> DownloadAsync(AssetDto asset, int? version = null)
        {
            var temp = new MemoryStream();

            using (var client = new HttpClient())
            {
                var url = $"{ServerUrl}{asset._links["content"].Href}";

                if (version > 0)
                {
                    url += $"?version={version}";
                }

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
                var upload = new FileParameter(stream, fileName ?? RandomName(fileInfo.Extension), asset.MimeType);

                return await Assets.PutAssetContentAsync(AppName, asset.Id, upload);
            }
        }

        public async Task<AssetDto> UploadFileAsync(string path, string mimeType, string fileName = null, string parentId = null, string id = null)
        {
            var fileInfo = new FileInfo(path);

            using (var stream = fileInfo.OpenRead())
            {
                var upload = new FileParameter(stream, fileName ?? RandomName(fileInfo.Extension), mimeType);

                return await Assets.PostAssetAsync(AppName, parentId, id, true, upload);
            }
        }

        public async Task<AssetDto> UploadFileAsync(int size, string fileName = null, string parentId = null, string id = null)
        {
            using (var stream = RandomAsset(size))
            {
                var upload = new FileParameter(stream, fileName ?? RandomName(".txt"), "text/csv");

                return await Assets.PostAssetAsync(AppName, parentId, id, true, upload);
            }
        }

        private static MemoryStream RandomAsset(int length)
        {
            var stream = new MemoryStream(length);

            var random = new Random();

            for (var i = 0; i < length; i++)
            {
                stream.WriteByte((byte)random.Next());
            }

            stream.Position = 0;

            return stream;
        }

        private static string RandomName(string extension)
        {
            var fileName = $"{Guid.NewGuid()}{extension}";

            return fileName;
        }
    }
}
