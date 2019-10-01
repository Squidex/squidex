// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class UIFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new UIFieldProperties());

            Assert.Equal("my-ui", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_value_is_undefined()
        {
            var sut = Field(new UIFieldProperties());

            await sut.ValidateAsync(Undefined.Value, errors, ValidationTestExtensions.ValidContext);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_value_is_json_null()
        {
            var sut = Field(new UIFieldProperties());

            await sut.ValidateAsync(JsonValue.Null, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Value must not be defined." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_valid()
        {
            var sut = Field(new UIFieldProperties { IsRequired = true });

            await sut.ValidateAsync(JsonValue.True, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Value must not be defined." });
        }

        [Fact]
        public async Task Should_add_error_if_field_object_is_defined()
        {
            var schema =
                new Schema("my-schema")
                    .AddUI(1, "my-ui1", Partitioning.Invariant)
                    .AddUI(2, "my-ui2", Partitioning.Invariant);

            var data =
                new NamedContentData()
                    .AddField("my-ui1", new ContentFieldData())
                    .AddField("my-ui2", new ContentFieldData()
                        .AddValue("iv", null));

            var validationContext = ValidationTestExtensions.ValidContext;
            var validator = new ContentValidator(schema, x => InvariantPartitioning.Instance, validationContext);

            await validator.ValidateAsync(data);

            validator.Errors.Should().BeEquivalentTo(
                new[]
                {
                    new ValidationError("Value must not be defined.", "my-ui1"),
                    new ValidationError("Value must not be defined.", "my-ui2")
                });
        }

        [Fact]
        public async Task Should_add_error_if_array_item_field_is_defined()
        {
            var schema =
                new Schema("my-schema")
                    .AddArray(1, "my-array", Partitioning.Invariant, array => array
                        .AddUI(101, "my-ui"));

            var data =
                new NamedContentData()
                    .AddField("my-array", new ContentFieldData()
                        .AddValue("iv",
                            JsonValue.Array(
                                JsonValue.Object()
                                    .Add("my-ui", null))));

            var validationContext =
                new ValidationContext(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    (c, s) => null!,
                    (c) => null!);

            var validator = new ContentValidator(schema, x => InvariantPartitioning.Instance, validationContext);

            await validator.ValidateAsync(data);

            validator.Errors.Should().BeEquivalentTo(
                new[] { new ValidationError("Value must not be defined.", "my-array[1].my-ui") });
        }

        private static NestedField<UIFieldProperties> Field(UIFieldProperties properties)
        {
            return new NestedField<UIFieldProperties>(1, "my-ui", properties);
        }
    }
}
