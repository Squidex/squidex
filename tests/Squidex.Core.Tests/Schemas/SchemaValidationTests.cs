// ==========================================================================
//  SchemaValidationTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Core.Contents;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core.Schemas
{
    public class SchemaValidationTests
    {
        private readonly HashSet<Language> languages = new HashSet<Language>(new[] { Language.GetLanguage("de"), Language.GetLanguage("en") });
        private readonly List<ValidationError> errors = new List<ValidationError>();
        private Schema sut = Schema.Create("my-name", new SchemaProperties());

        [Fact]
        public async Task Should_add_error_if_validating_data_with_unknown_field()
        {
            var data =
                ContentData.Empty()
                    .AddField("unknown",
                        ContentFieldData.New());

            await sut.ValidateAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("unknown is not a known field", "unknown")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_data_with_invalid_field()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { MaxValue = 100 }));

            var data =
                ContentData.Empty()
                    .AddField("my-field",
                        ContentFieldData.New()
                            .AddValue(1000));

            await sut.ValidateAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field must be less than '100'", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_non_localizable_field_contains_language()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties()));

            var data =
                ContentData.Empty()
                    .AddField("my-field",
                        ContentFieldData.New()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await sut.ValidateAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field can only contain a single entry for invariant language (iv)", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_data_with_invalid_localizable_field()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsRequired = true, IsLocalizable = true }));

            var data =
                ContentData.Empty();

            await sut.ValidateAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field (de) is required", "my-field"),
                    new ValidationError("my-field (en) is required", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_required_field_is_not_in_bag()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsRequired = true }));

            var data =
                ContentData.Empty();

            await sut.ValidateAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field is required", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_value_contains_invalid_language()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsLocalizable = true }));

            var data =
                ContentData.Empty()
                    .AddField("my-field",
                        ContentFieldData.New()
                            .AddValue("de", 1)
                            .AddValue("xx", 1));

            await sut.ValidateAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field has an invalid language 'xx'", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_value_contains_unsupported_language()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsLocalizable = true }));

            var data =
                ContentData.Empty()
                    .AddField("my-field",
                        ContentFieldData.New()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await sut.ValidateAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field has an unsupported language 'es'", "my-field"),
                    new ValidationError("my-field has an unsupported language 'it'", "my-field")
                });
        }
    }
}
