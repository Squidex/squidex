// ==========================================================================
//  GuardAppPatternsTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Apps.Guards
{
    public class GuardAppPatternsTests
    {
        private AppPatterns patterns = AppPatterns.Empty;

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
        public void CanCreate_should_throw_exception_if_pattern_empty()
        {
            var command = new AddPattern
            {
                Name = "Pattern",
                Pattern = string.Empty
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_name_exists()
        {
            patterns = patterns.Add("Pattern", "[a-z]", "Message");
            var command = new AddPattern
            {
                Name = "Pattern",
                Pattern = "[0-9]"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_pattern_exists()
        {
            patterns = patterns.Add("Pattern 2", new AppPattern { Name = "Pattern 2", Pattern = "[0-9]", DefaultMessage = "Message" });
            var command = new AddPattern
            {
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
                Name = "Pattern"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanDelete_should_not_throw_exception_if_success()
        {
            patterns = patterns.Add("Pattern", "[0-9]", "Message");
            var command = new DeletePattern
            {
                Name = "Pattern"
            };

            GuardAppPattern.CanApply(patterns, command);
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_name_empty()
        {
            var command = new UpdatePattern
            {
                Name = string.Empty,
                OriginalName = "Pattern",
                Pattern = "[0-9]"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_pattern_empty()
        {
            var command = new UpdatePattern
            {
                Name = "Pattern",
                OriginalName = "Pattern 2",
                Pattern = string.Empty
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_orignal_name_empty()
        {
            var command = new UpdatePattern
            {
                Name = string.Empty,
                OriginalName = "Pattern",
                Pattern = string.Empty
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_name_exists()
        {
            patterns = patterns.Add("Pattern", "[a-z]", "Message");
            var command = new UpdatePattern
            {
                Name = "Pattern",
                OriginalName = "Pattern 1",
                Pattern = "[0-9]"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_pattern_exists()
        {
            patterns = patterns.Add("Pattern 2", "[0-9]", "Message");
            var command = new UpdatePattern
            {
                Name = "Pattern",
                OriginalName = "Pattern",
                Pattern = "[0-9]"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_pattern_not_found()
        {
            var command = new UpdatePattern
            {
                Name = "Pattern",
                OriginalName = "Pattern",
                Pattern = "[0-9]"
            };

            Assert.Throws<ValidationException>(() => GuardAppPattern.CanApply(patterns, command));
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_name_changed_pattern_does_not()
        {
            patterns = patterns.Add("Pattern", "[0-9]", "Message");
            var command = new UpdatePattern
            {
                Name = "Pattern Update",
                OriginalName = "Pattern",
                Pattern = "[0-9]"
            };

            GuardAppPattern.CanApply(patterns, command);
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_pattern_changed_name_not()
        {
            patterns = patterns.Add("Pattern", "[0-9]", "Message");
            var command = new UpdatePattern
            {
                Name = "Pattern",
                OriginalName = "Pattern",
                Pattern = "[0-9a-z]"
            };

            GuardAppPattern.CanApply(patterns, command);
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_pattern_and_name_changed()
        {
            patterns = patterns.Add("Pattern", "[0-9]", "Message");
            var command = new UpdatePattern
            {
                Name = "Pattern 2",
                OriginalName = "Pattern",
                Pattern = "[0-9a-z]"
            };

            GuardAppPattern.CanApply(patterns, command);
        }
    }
}
