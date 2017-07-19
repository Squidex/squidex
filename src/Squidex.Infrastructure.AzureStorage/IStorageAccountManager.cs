// ==========================================================================
//  IStorageAccountManager.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.WindowsAzure.Storage.Blob;

namespace Squidex.Infrastructure.AzureStorage
{
    public interface IStorageAccountManager
    {
        CloudBlobClient CreateCloudBlobClient();

        string GetSharedAccessSignature();
    }
}
