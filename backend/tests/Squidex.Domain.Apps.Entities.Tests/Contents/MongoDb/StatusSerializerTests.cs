// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb
{
    public sealed class StatusSerializerTests
    {
        private sealed class TestObject
        {
            public Status Status { get; set; }
        }

        [Fact]
        public void Should_serialize_and_deserialize_status()
        {
            StatusSerializer.Register();

            var source = new TestObject
            {
                Status = Status.Published
            };

            var document = new BsonDocument();

            using (var writer = new BsonDocumentWriter(document))
            {
                BsonSerializer.Serialize(writer, source);

                writer.Flush();
            }

            using (var reader = new BsonDocumentReader(document))
            {
                var result = BsonSerializer.Deserialize<TestObject>(reader);

                Assert.Equal(source.Status, result.Status);
            }
        }
    }
}
