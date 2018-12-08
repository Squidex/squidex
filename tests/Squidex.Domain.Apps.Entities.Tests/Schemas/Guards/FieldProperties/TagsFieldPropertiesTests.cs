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
    public class TagsFieldPropertiesTests
    {
        [Fact]
        public void Should_add_error_if_min_greater_than_max()
        {
            var sut = new TagsFieldProperties { MinItems = 10, MaxItems = 5 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Max items must be greater than min items.", "MinItems", "MaxItems")
                });
        }

        [Fact]
        public void Should_add_error_if_radio_button_has_no_allowed_values()
        {
            var sut = new TagsFieldProperties { Editor = TagsFieldEditor.Checkboxes };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Checkboxes or dropdown list need allowed values.", "AllowedValues")
                });
        }

        [Fact]
        public void Should_add_error_if_editor_is_not_valid()
        {
            var sut = new TagsFieldProperties { Editor = (TagsFieldEditor)123 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Editor is not a valid value.", "Editor")
                });
        }
    }
}
