// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Infrastructure
{
    public class ValidationExceptionTests
    {
        [Fact]
        public void Should_format_message_from_error()
        {
            var ex = new ValidationException("Error.");

            Assert.Equal("Error.", ex.Message);
        }

        [Fact]
        public void Should_append_dot_to_error()
        {
            var ex = new ValidationException("Error");

            Assert.Equal("Error.", ex.Message);
        }

        [Fact]
        public void Should_format_message_from_errors()
        {
            var errors = new List<ValidationError>
            {
                new ValidationError("Error1"),
                new ValidationError("Error2")
            };

            var ex = new ValidationException(errors);

            Assert.Equal("Error1. Error2.", ex.Message);
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var errors = new List<ValidationError>
            {
                new ValidationError("Error1"),
                new ValidationError("Error2")
            };

            var source = new ValidationException(errors);
            var result = source.SerializeAndDeserializeBinary();

            result.Errors.Should().BeEquivalentTo(source.Errors);

            Assert.Equal(source.Message, result.Message);
        }
    }
}
