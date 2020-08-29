// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppPatternJsonTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var patterns = AppPatterns.Empty;

            var guid1 = DomainId.NewGuid();
            var guid2 = DomainId.NewGuid();
            var guid3 = DomainId.NewGuid();

            patterns = patterns.Add(guid1, "Name1", "Pattern1", "Default");
            patterns = patterns.Add(guid2, "Name2", "Pattern2", "Default");
            patterns = patterns.Add(guid3, "Name3", "Pattern3", "Default");
            patterns = patterns.Update(guid2, "Name2 Update", "Pattern2 Update", "Default2");
            patterns = patterns.Update(guid3, "Name3 Update", "Pattern3 Update", "Default3");
            patterns = patterns.Remove(guid1);

            var serialized = patterns.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(patterns);
        }
    }
}
