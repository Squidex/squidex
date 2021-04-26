// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class UIFieldTests : IClassFixture<TranslationsFixture>
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

            await sut.ValidateAsync(Undefined.Value, errors);

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
                new ContentData()
                    .AddField("my-ui1", new ContentFieldData())
                    .AddField("my-ui2", new ContentFieldData()
                        .AddInvariant(null));

            var dataErrors = new List<ValidationError>();

            await data.ValidateAsync(x => InvariantPartitioning.Instance, dataErrors, schema);

            dataErrors.Should().BeEquivalentTo(
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
                new ContentData()
                    .AddField("my-array", new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object()
                                    .Add("my-ui", null))));

            var dataErrors = new List<ValidationError>();

            await data.ValidateAsync(x => InvariantPartitioning.Instance, dataErrors, schema);

            dataErrors.Should().BeEquivalentTo(
                new[]
                {
                    new ValidationError("Value must not be defined.", "my-array.iv[1].my-ui")
                });
        }

        private static NestedField<UIFieldProperties> Field(UIFieldProperties properties)
        {
            return new NestedField<UIFieldProperties>(1, "my-ui", properties);
        }
    }
}
