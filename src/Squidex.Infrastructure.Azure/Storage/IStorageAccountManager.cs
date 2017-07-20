// ==========================================================================
//  IStorageAccountManager.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Squidex.Infrastructure.Azure.Storage
{
    public interface IStorageAccountManager
    {
        CloudBlobClient CreateCloudBlobClient();

        string GetSharedAccessSignature();

        CloudBlobContainer GetContainer(string name);
    }
}
