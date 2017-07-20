// ==========================================================================
//  IStorageAccountManager.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Squidex.Infrastructure.Azure.Storage
{
    public class StorageAccountManager : IStorageAccountManager
    {
        private readonly CloudStorageAccount storageAccount;

        public StorageAccountManager(string storageAccountConnectionString)
        {
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            }
            catch (Exception ex)
                when (ex is FormatException || ex is ArgumentException)
            {
                throw new AzureStorageException("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app settings file.");
            }
        }

        public CloudBlobClient CreateCloudBlobClient()
        {
            return storageAccount.CreateCloudBlobClient();
        }

        public string GetSharedAccessSignature()
        {
            return storageAccount.GetSharedAccessSignature(new SharedAccessAccountPolicy()
            {
                SharedAccessStartTime = DateTimeOffset.UtcNow,
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(1),
                Permissions = SharedAccessAccountPermissions.Read | SharedAccessAccountPermissions.List
            });
        }

        public CloudBlobContainer GetContainer(string name)
        {
            var blobClient = CreateCloudBlobClient();
            return blobClient.GetContainerReference(name);
        }
    }
}