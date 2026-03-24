// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.MongoDb.TestHelpers;

[assembly: AssemblyFixture(typeof(MongoSerializerFixture))]

namespace Squidex.MongoDb.TestHelpers;

internal sealed class MongoSerializerFixture : IDisposable
{
    public MongoSerializerFixture()
    {
        MongoClientFactory.SetupSerializer(TestUtils.DefaultOptions(), BsonType.String);
    }

    public void Dispose()
    {
    }
}
