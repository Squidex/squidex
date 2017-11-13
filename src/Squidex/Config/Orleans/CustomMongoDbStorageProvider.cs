// ==========================================================================
//  CustomMongoDbStorageProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Orleans.Providers;
using Orleans.Providers.MongoDB.StorageProviders;
using Squidex.Config.Domain;

namespace Squidex.Config.Orleans
{
    public sealed class CustomMongoDbStorageProvider : MongoStorageProvider
    {
        protected override JsonSerializerSettings ReturnSerializerSettings(IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            return SerializationServices.DefaultJsonSettings;
        }
    }
}