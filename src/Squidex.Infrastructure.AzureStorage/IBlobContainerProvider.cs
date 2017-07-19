// ==========================================================================
//  IBlobContainerProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Squidex.Infrastructure.AzureStorage
{
    public interface IBlobContainerProvider
    {
        Task<CloudBlobContainer> GetContainerAsync(string name);
    }
}
