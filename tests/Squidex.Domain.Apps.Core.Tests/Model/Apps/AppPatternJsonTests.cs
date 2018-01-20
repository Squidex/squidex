// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppPatternJsonTests
    {
        private readonly JsonSerializer serializer = TestData.DefaultSerializer();

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var patterns = AppPatterns.Empty;

            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            patterns = patterns.Add(guid1, "Name1", "Pattern1", "Default");
            patterns = patterns.Add(guid2, "Name2", "Pattern2", "Default");
            patterns = patterns.Add(guid3, "Name3", "Pattern3", "Default");

            patterns = patterns.Update(guid2, "Name2 Update", "Pattern2 Update", "Default2");

            patterns = patterns.Remove(guid1);

            var appPatterns = JToken.FromObject(patterns, serializer).ToObject<AppPatterns>(serializer);

            appPatterns.ShouldBeEquivalentTo(patterns);
        }
    }
}
