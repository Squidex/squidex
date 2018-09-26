// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Guards.FieldProperties
{
    public class BooleanFieldPropertiesTests
    {
        [Fact]
        public void Should_add_error_if_editor_is_not_valid()
        {
            var sut = new BooleanFieldProperties { Editor = (BooleanFieldEditor)123 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Editor is not a valid value.", "Editor")
                });
        }
    }
}
