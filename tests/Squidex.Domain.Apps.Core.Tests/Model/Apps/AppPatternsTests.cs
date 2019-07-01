// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppPatternsTests
    {
        private readonly AppPatterns patterns_0;
        private readonly Guid firstId = Guid.NewGuid();
        private readonly Guid id = Guid.NewGuid();

        public AppPatternsTests()
        {
            patterns_0 = AppPatterns.Empty.Add(firstId, "Default", "Default Pattern", "Message");
        }

        [Fact]
        public void Should_add_pattern()
        {
            var patterns_1 = patterns_0.Add(id, "NewPattern", "New Pattern", "Message");

            patterns_1[id].Should().BeEquivalentTo(new AppPattern("NewPattern", "New Pattern", "Message"));
        }

        [Fact]
        public void Should_throw_exception_if_add_pattern_with_same_id()
        {
            var patterns_1 = patterns_0.Add(id, "NewPattern", "New Pattern", "Message");

            Assert.Throws<ArgumentException>(() => patterns_1.Add(id, "NewPattern", "New Pattern", "Message"));
        }

        [Fact]
        public void Should_update_pattern()
        {
            var patterns_1 = patterns_0.Update(firstId, "UpdatePattern", "Update Pattern", "Message");

            patterns_1[firstId].Should().BeEquivalentTo(new AppPattern("UpdatePattern", "Update Pattern", "Message"));
        }

        [Fact]
        public void Should_return_same_patterns_if_pattern_not_found()
        {
            var patterns_1 = patterns_0.Update(id, "NewPattern", "NewPattern", "Message");

            Assert.Same(patterns_0, patterns_1);
        }

        [Fact]
        public void Should_remove_pattern()
        {
            var patterns_1 = patterns_0.Remove(firstId);

            Assert.Empty(patterns_1);
        }

        [Fact]
        public void Should_do_nothing_if_remove_pattern_not_found()
        {
            var patterns_1 = patterns_0.Remove(id);

            Assert.NotEmpty(patterns_1);
        }
    }
}
