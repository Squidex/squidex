// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;

namespace Squidex.Infrastructure;

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
}
