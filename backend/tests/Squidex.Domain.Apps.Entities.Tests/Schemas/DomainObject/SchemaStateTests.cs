// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject
{
    public class SchemaStateTests
    {
        [Fact]
        public void Should_deserialize_state()
        {
            var json = File.ReadAllText("Schemas/DomainObject/SchemaState.json");

            var deserialized = TestUtils.DefaultSerializer.Deserialize<SchemaDomainObject.State>(json);

            Assert.NotNull(deserialized);
        }

        [Fact]
        public void Should_serialize_deserialize_state()
        {
            var json = File.ReadAllText("Schemas/DomainObject/SchemaState.json").CleanJson();

            var serialized = TestUtils.DeserializeAndSerialize<SchemaDomainObject.State>(json);

            Assert.Equal(json, serialized);
        }
    }
}
