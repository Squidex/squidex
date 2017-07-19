// ==========================================================================
//  BlobContainerProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Squidex.Infrastructure.AzureStorage
{
    public class BlobContainerProvider : IBlobContainerProvider
    {
        private readonly IStorageAccountManager accountManager;

        public BlobContainerProvider(IStorageAccountManager accountManager)
        {
            this.accountManager = accountManager;
        }

        public async Task<CloudBlobContainer> GetContainerAsync(string name)
        {
            var client = accountManager.CreateCloudBlobClient();
            var saneName = name.Replace("@", "-").Replace(".", "-");
            var container = client.GetContainerReference(saneName);
            await container.CreateIfNotExistsAsync();

            return container;
        }
    }
}
