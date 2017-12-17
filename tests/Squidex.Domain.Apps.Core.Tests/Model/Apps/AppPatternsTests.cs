// ==========================================================================
//  AppClientsTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppPatternsTests
    {
        private readonly AppPatterns defaultPatterns;
        private readonly Guid firstId = Guid.NewGuid();
        private readonly Guid id = Guid.NewGuid();

        public AppPatternsTests()
        {
            defaultPatterns = AppPatterns.Empty.Add(firstId, "Default", "Default Pattern", "Message");

            id = Guid.NewGuid();
        }

        [Fact]
        public void Should_add_pattern()
        {
            var patterns = defaultPatterns.Add(id, "NewPattern", "New Pattern", "Message");

            patterns[id].ShouldBeEquivalentTo(new AppPattern(id, "NewPattern", "New Pattern", "Message"));
        }

        [Fact]
        public void Should_throw_exception_if_add_pattern_with_same_id()
        {
            var patterns = defaultPatterns.Add(id, "NewPattern", "New Pattern", "Message");

            Assert.Throws<ArgumentException>(() => patterns.Add(id, "NewPattern", "New Pattern", "Message"));
        }

        [Fact]
        public void Should_update_pattern()
        {
            var patterns = defaultPatterns.Update(firstId, "UpdatePattern", "Update Pattern", "Message");

            patterns[firstId].ShouldBeEquivalentTo(new AppPattern(firstId, "UpdatePattern", "Update Pattern", "Message"));
        }

        [Fact]
        public void Should_return_same_patterns_if_pattern_not_found()
        {
            var patterns = defaultPatterns.Update(id, "NewPattern", "NewPattern", "Message");

            Assert.Same(defaultPatterns, patterns);
        }

        [Fact]
        public void Should_remove_pattern()
        {
            var patterns = defaultPatterns.Remove(firstId);

            Assert.Empty(patterns);
        }

        [Fact]
        public void Should_do_nothing_if_remove_pattern_not_found()
        {
            var patterns = defaultPatterns.Remove(id);

            Assert.NotSame(defaultPatterns, patterns);
        }
    }
}
