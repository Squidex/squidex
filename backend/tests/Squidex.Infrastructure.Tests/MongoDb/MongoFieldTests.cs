// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Xunit;

namespace Squidex.Infrastructure.MongoDb
{
    public class MongoFieldTests
    {
        public sealed class Entity
        {
            public string Id { get; set; }

            public string Default { get; set; }

            [BsonElement("_c")]
            public string Custom { get; set; }
        }

        [Fact]
        public void Should_resolve_id_field()
        {
            var name = Field.Of<Entity>(x => nameof(x.Id));

            Assert.Equal("_id", name);
        }

        [Fact]
        public void Should_resolve_default_field()
        {
            var name = Field.Of<Entity>(x => nameof(x.Default));

            Assert.Equal("Default", name);
        }

        [Fact]
        public void Should_resolve_custom_field()
        {
            var name = Field.Of<Entity>(x => nameof(x.Custom));

            Assert.Equal("_c", name);
        }
    }
}
