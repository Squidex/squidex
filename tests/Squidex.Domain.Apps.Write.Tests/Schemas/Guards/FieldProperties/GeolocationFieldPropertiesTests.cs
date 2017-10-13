// ==========================================================================
//  GeolocationPropertiesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Schemas.Guards.FieldProperties
{
    public class GeolocationFieldPropertiesTests
    {
        [Fact]
        public void Should_add_error_if_editor_is_not_valid()
        {
            var sut = new GeolocationFieldProperties { Editor = (GeolocationFieldEditor)123 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Editor is not a valid value.", "Editor")
                });
        }
    }
}
