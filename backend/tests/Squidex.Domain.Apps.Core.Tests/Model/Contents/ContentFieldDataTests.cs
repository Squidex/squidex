// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Contents
{
    public class ContentFieldDataTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var fieldData =
                new ContentFieldData()
                    .AddValue(12);

            var serialized = fieldData.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(fieldData);
        }

        [Fact]
        public void Should_intern_invariant_key()
        {
            var fieldData =
                new ContentFieldData()
                    .AddValue(12);

            var serialized = fieldData.SerializeAndDeserialize();

            Assert.NotNull(string.IsInterned(serialized.Keys.First()));
        }

        [Fact]
        public void Should_intern_known_language()
        {
            var fieldData =
                new ContentFieldData()
                    .AddValue("en", 12);

            var serialized = fieldData.SerializeAndDeserialize();

            Assert.NotNull(string.IsInterned(serialized.Keys.First()));
        }

        [Fact]
        public void Should_not_intern_unknown_key()
        {
            var fieldData =
                new ContentFieldData()
                    .AddValue(Guid.NewGuid().ToString(), 12);

            var serialized = fieldData.SerializeAndDeserialize();

            Assert.Null(string.IsInterned(serialized.Keys.First()));
        }
    }
}
