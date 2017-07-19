// ==========================================================================
//  AzureStorageException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.WindowsAzure.Storage;

namespace Squidex.Infrastructure.AzureStorage
{
    [Serializable]
    public class AzureStorageException : StorageException
    {
        public AzureStorageException()
        {
        }

        public AzureStorageException(string message) : base(message)
        {
        }

        public AzureStorageException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
