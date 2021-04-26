// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject.Guards.FieldProperties
{
    public class UIFieldPropertiesTests : IClassFixture<TranslationsFixture>
    {
        [Fact]
        public void Should_not_add_error_if_editor_is_correct()
        {
            var sut = new UIFieldProperties { Editor = UIFieldEditor.Separator };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            Assert.Empty(errors);
        }

        [Fact]
        public void Should_add_error_if_editor_is_not_valid()
        {
            var sut = new UIFieldProperties { Editor = (UIFieldEditor)123 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Editor is not a valid value.", "Editor")
                });
        }
    }
}
