// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards
{
    public class ScriptMetadataWrapperTests
    {
        private readonly AssetMetadata metadata = new AssetMetadata();
        private readonly ScriptMetadataWrapper sut;

        public ScriptMetadataWrapperTests()
        {
            sut = new ScriptMetadataWrapper(metadata);
        }

        [Fact]
        public void Should_add_value()
        {
            sut.Add("key", 1);

            Assert.Equal(JsonValue.Create(1), metadata["key"]);
            Assert.Equal(JsonValue.Create(1), sut["key"]);

            Assert.True(metadata.ContainsKey("key"));
            Assert.True(sut.ContainsKey("key"));

            Assert.Single(metadata);
            Assert.Single(sut);
        }

        [Fact]
        public void Should_set_value()
        {
            sut["key"] = 1;

            Assert.Equal(JsonValue.Create(1), metadata["key"]);
            Assert.Equal(JsonValue.Create(1), sut["key"]);

            Assert.True(metadata.ContainsKey("key"));
            Assert.True(sut.ContainsKey("key"));

            Assert.Single(metadata);
            Assert.Single(sut);
        }

        [Fact]
        public void Should_provide_keys()
        {
            sut["key1"] = 1;
            sut["key2"] = 2;

            Assert.Equal(new[]
            {
                "key1",
                "key2"
            }, sut.Keys.ToArray());
        }

        [Fact]
        public void Should_provide_values()
        {
            sut["key1"] = 1;
            sut["key2"] = 2;

            Assert.Equal(new object[]
            {
                JsonValue.Create(1),
                JsonValue.Create(2)
            }, sut.Values.ToArray());
        }

        [Fact]
        public void Should_enumerate_values()
        {
            sut["key1"] = 1;
            sut["key2"] = 2;

            Assert.Equal(new[]
            {
                new KeyValuePair<string, object?>("key1", JsonValue.Create(1)),
                new KeyValuePair<string, object?>("key2", JsonValue.Create(2)),
            }, sut.ToArray());
        }

        [Fact]
        public void Should_remove_value()
        {
            sut["key"] = 1;
            sut.Remove("key");

            Assert.Empty(metadata);
            Assert.Empty(sut);
        }

        [Fact]
        public void Should_clear_collection()
        {
            sut["key"] = 1;
            sut.Clear();

            Assert.Empty(metadata);
            Assert.Empty(sut);
        }
    }
}
