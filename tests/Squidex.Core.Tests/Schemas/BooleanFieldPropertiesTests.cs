// ==========================================================================
//  BooleanFieldPropertiesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core.Schemas
{
    public class BooleanFieldPropertiesTests
    {
        private readonly List<ValidationError> errors = new List<ValidationError>();

        [Fact]
        public void Should_add_error_if_editor_is_not_valid()
        {
            var sut = new BooleanFieldProperties { Editor = (BooleanFieldEditor)123 };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Editor ist not a valid value", "Editor")
                });
        }
    }
}
