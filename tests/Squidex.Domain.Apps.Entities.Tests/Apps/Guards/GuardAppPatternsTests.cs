// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public class GuardAppPatternsTests
    {
        [Fact]
        public void CanConfigure_should_throw_exception_if_two_patterns_with_same_pattern_exist()
        {
            var command = new ConfigurePatterns
            {
                Patterns = new[]
                {
                    new UpsertAppPattern { Name = "name1", Pattern = "[a-z]" },
                    new UpsertAppPattern { Name = "name2", Pattern = "[a-z]" }
                }
            };

            ValidationAssert.Throws(() => GuardAppPatterns.CanConfigure(command),
                new ValidationError("Two patterns with the same expression exist.", "Patterns"));
        }

        [Fact]
        public void CanConfigure_should_throw_exception_if_two_patterns_with_same_name_exist()
        {
            var command = new ConfigurePatterns
            {
                Patterns = new[]
                {
                    new UpsertAppPattern { Name = "name", Pattern = "[a-z]" },
                    new UpsertAppPattern { Name = "name", Pattern = "[0-9]" }
                }
            };

            ValidationAssert.Throws(() => GuardAppPatterns.CanConfigure(command),
                new ValidationError("Two patterns with the same name exist.", "Patterns"));
        }

        [Fact]
        public void CanConfigure_should_throw_exception_if_expression_not_valid()
        {
            var command = new ConfigurePatterns
            {
                Patterns = new[]
                {
                    new UpsertAppPattern { Name = "name", Pattern = "((" }
                }
            };

            ValidationAssert.Throws(() => GuardAppPatterns.CanConfigure(command),
                new ValidationError("Expression is not a valid value.", "Patterns[1].Pattern"));
        }

        [Fact]
        public void CanConfigure_should_throw_exception_if_expression_is_empty()
        {
            var command = new ConfigurePatterns
            {
                Patterns = new[]
                {
                    new UpsertAppPattern { Name = "name" }
                }
            };

            ValidationAssert.Throws(() => GuardAppPatterns.CanConfigure(command),
                new ValidationError("Expression is required.", "Patterns[1].Pattern"));
        }

        [Fact]
        public void CanConfigure_should_throw_exception_if_name_is_empty()
        {
            var command = new ConfigurePatterns
            {
                Patterns = new[]
                {
                    new UpsertAppPattern { Pattern = "[0-9]" }
                }
            };

            ValidationAssert.Throws(() => GuardAppPatterns.CanConfigure(command),
                new ValidationError("Name is required.", "Patterns[1].Name"));
        }

        [Fact]
        public void CanConfigure_should_throw_exception_if_pattern_is_null()
        {
            var command = new ConfigurePatterns
            {
                Patterns = new UpsertAppPattern[]
                {
                    null
                }
            };

            ValidationAssert.Throws(() => GuardAppPatterns.CanConfigure(command),
                new ValidationError("Pattern is required.", "Patterns[1]"));
        }

        [Fact]
        public void CanConfigure_should_not_throw_exception_if_patterns_is_valid()
        {
            var command = new ConfigurePatterns
            {
                Patterns = new[]
                {
                    new UpsertAppPattern { Name = "number", Pattern = "[0-9]" }
                }
            };

            GuardAppPatterns.CanConfigure(command);
        }

        [Fact]
        public void CanConfigure_should_not_throw_exception_if_patterns_is_null()
        {
            var command = new ConfigurePatterns();

            GuardAppPatterns.CanConfigure(command);
        }

        [Fact]
        public void CanConfigure_should_not_throw_exception_if_patterns_is_empty()
        {
            var command = new ConfigurePatterns { Patterns = new UpsertAppPattern[0] };

            GuardAppPatterns.CanConfigure(command);
        }
    }
}
