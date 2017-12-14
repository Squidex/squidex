// ==========================================================================
//  GuardAppPatternsTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Apps.Guards
{
    public class GuardAppPatternsTests
    {
        private AppPatterns patterns = AppPatterns.Empty.Add(Guid.NewGuid(), "Default Pattern", "[A-z]", "Message");

        [Fact]
        public void CanCreate_should_throw_exception_if_name_empty()
        {
            var command = new AddPattern
            {
                Name = string.Empty,
                Pattern = "[0-9]"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_id_empty_guid()
        {
            var command = new AddPattern
            {
                Name = "Pattern",
                Pattern = "Pattern"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_pattern_empty()
        {
            var command = new AddPattern
            {
                Id = Guid.NewGuid(),
                Name = "Pattern",
                Pattern = string.Empty
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_name_exists()
        {
            patterns = patterns.Add(Guid.NewGuid(), "Pattern", "[a-z]", "Message");
            var command = new AddPattern
            {
                Id = Guid.NewGuid(),
                Name = "Pattern",
                Pattern = "[0-9]"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanCreate_should_not_throw_exception_if_success()
        {
            var command = new AddPattern
            {
                Id = Guid.NewGuid(),
                Name = "Pattern",
                Pattern = "[0-9]"
            };

            GuardAppPattern.CanApply(patterns, command);
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_pattern_not_found()
        {
            var command = new DeletePattern
            {
                Id = Guid.NewGuid()
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanDelete_should_not_throw_exception_if_success()
        {
            Guid id = Guid.NewGuid();
            patterns = patterns.Add(id, "Pattern", "[0-9]", "Message");
            var command = new DeletePattern
            {
                Id = id
            };

            GuardAppPattern.CanApply(patterns, command);
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_name_empty()
        {
            var command = new UpdatePattern
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Pattern = "[0-9]"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_pattern_empty()
        {
            var command = new UpdatePattern
            {
                Id = Guid.NewGuid(),
                Name = "Pattern",
                Pattern = string.Empty
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_name_exists()
        {
            Guid id = Guid.NewGuid();
            patterns = patterns.Add(id, "Pattern", "[a-z]", "Message");
            var command = new UpdatePattern
            {
                Id = Guid.NewGuid(),
                Name = "Pattern",
                Pattern = "[0-9]"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_pattern_exists()
        {
            Guid id = Guid.NewGuid();
            patterns = patterns.Add(id, "Pattern 2", "[0-9]", "Message");
            var command = new UpdatePattern
            {
                Id = Guid.NewGuid(),
                Name = "Pattern",
                Pattern = "[0-9]"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_pattern_not_found()
        {
            var command = new UpdatePattern
            {
                Id = Guid.NewGuid(),
                Name = "Pattern",
                Pattern = "[0-9]"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_name_changed_pattern_does_not()
        {
            Guid id = Guid.NewGuid();
            patterns = patterns.Add(id, "Pattern", "[0-9]", "Message");
            var command = new UpdatePattern
            {
                Id = id,
                Name = "Pattern Update",
                Pattern = "[0-9]"
            };

            GuardAppPattern.CanApply(patterns, command);
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_pattern_changed_name_not()
        {
            Guid id = Guid.NewGuid();
            patterns = patterns.Add(id, "Pattern", "[0-9]", "Message");
            var command = new UpdatePattern
            {
                Id = id,
                Name = "Pattern",
                Pattern = "[0-9a-z]"
            };

            GuardAppPattern.CanApply(patterns, command);
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_pattern_and_name_changed()
        {
            Guid id = Guid.NewGuid();
            patterns = patterns.Add(id, "Pattern", "[0-9]", "Message");
            var command = new UpdatePattern
            {
                Id = id,
                Name = "Pattern 2",
                Pattern = "[0-9a-z]"
            };

            GuardAppPattern.CanApply(patterns, command);
        }
    }
}
