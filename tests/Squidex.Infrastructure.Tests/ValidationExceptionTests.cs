// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure
{
    public class ValidationExceptionTests
    {
        [Fact]
        public void Should_format_message_from_summary()
        {
            var ex = new ValidationException("Summary.");

            Assert.Equal("Summary.", ex.Message);
        }

        [Fact]
        public void Should_append_dot_to_summary()
        {
            var ex = new ValidationException("Summary");

            Assert.Equal("Summary.", ex.Message);
        }

        [Fact]
        public void Should_format_message_from_errors()
        {
            var ex = new ValidationException("Summary", new ValidationError("Error1."), new ValidationError("Error2."));

            Assert.Equal("Summary: Error1. Error2.", ex.Message);
        }

        [Fact]
        public void Should_not_add_colon_twice()
        {
            var ex = new ValidationException("Summary:", new ValidationError("Error1."), new ValidationError("Error2."));

            Assert.Equal("Summary: Error1. Error2.", ex.Message);
        }

        [Fact]
        public void Should_append_dots_to_errors()
        {
            var ex = new ValidationException("Summary", new ValidationError("Error1"), new ValidationError("Error2"));

            Assert.Equal("Summary: Error1. Error2.", ex.Message);
        }
    }
}
