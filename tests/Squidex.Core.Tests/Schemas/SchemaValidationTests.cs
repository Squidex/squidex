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
using NodaTime;
using NodaTime.Text;
using Squidex.Core.Contents;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core.Schemas
{
    public class SchemaValidationTests
    {
        private readonly HashSet<Language> languages = new HashSet<Language>(new[] { Language.DE, Language.EN });
        private readonly List<ValidationError> errors = new List<ValidationError>();
        private Schema sut = Schema.Create("my-name", new SchemaProperties());

        [Fact]
        public async Task Should_add_error_if_validating_data_with_unknown_field()
        {
            var data =
                new ContentData()
                    .AddField("unknown",
                        new ContentFieldData());

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
                new ContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .SetValue(1000));

            await sut.ValidateAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field must be less than '100'", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_non_localizable_data_field_contains_language()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties()));

            var data =
                new ContentData()
                    .AddField("my-field",
                        new ContentFieldData()
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
                new ContentData();

            await sut.ValidateAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field (de) is required", "my-field"),
                    new ValidationError("my-field (en) is required", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_required_data_field_is_not_in_bag()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsRequired = true }));

            var data =
                new ContentData();

            await sut.ValidateAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field is required", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_data_contains_invalid_language()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsLocalizable = true }));

            var data =
                new ContentData()
                    .AddField("my-field",
                        new ContentFieldData()
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
        public async Task Should_add_error_if_data_contains_unsupported_language()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsLocalizable = true }));

            var data =
                new ContentData()
                    .AddField("my-field",
                        new ContentFieldData()
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

        [Fact]
        public async Task Should_add_error_if_validating_partial_data_with_unknown_field()
        {
            var data =
                new ContentData()
                    .AddField("unknown",
                        new ContentFieldData());

            await sut.ValidatePartialAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("unknown is not a known field", "unknown")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_partial_data_with_invalid_field()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { MaxValue = 100 }));

            var data =
                new ContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .SetValue(1000));

            await sut.ValidatePartialAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field must be less than '100'", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_non_localizable_partial_data_field_contains_language()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties()));

            var data =
                new ContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await sut.ValidatePartialAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field can only contain a single entry for invariant language (iv)", "my-field")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_validating_partial_data_with_invalid_localizable_field()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsRequired = true, IsLocalizable = true }));

            var data =
                new ContentData();

            await sut.ValidatePartialAsync(data, errors, languages);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_not_add_error_if_required_partial_data_field_is_not_in_bag()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsRequired = true }));

            var data =
                new ContentData();

            await sut.ValidatePartialAsync(data, errors, languages);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_add_error_if_partial_data_contains_invalid_language()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsLocalizable = true }));

            var data =
                new ContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("xx", 1));

            await sut.ValidatePartialAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field has an invalid language 'xx'", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_partial_data_contains_unsupported_language()
        {
            sut = sut.AddOrUpdateField(new NumberField(1, "my-field", new NumberFieldProperties { IsLocalizable = true }));

            var data =
                new ContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await sut.ValidatePartialAsync(data, errors, languages);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field has an unsupported language 'es'", "my-field"),
                    new ValidationError("my-field has an unsupported language 'it'", "my-field")
                });
        }

        [Fact]
        private void Should_enrich_with_default_values()
        {
            var now = Instant.FromUnixTimeSeconds(SystemClock.Instance.GetCurrentInstant().ToUnixTimeSeconds());

            var schema =
                Schema.Create("my-schema", new SchemaProperties())
                    .AddOrUpdateField(new JsonField(1, "my-json", 
                        new JsonFieldProperties()))
                    .AddOrUpdateField(new StringField(2, "my-string", 
                        new StringFieldProperties { DefaultValue = "EN-String", IsLocalizable = true }))
                    .AddOrUpdateField(new NumberField(3, "my-number", 
                        new NumberFieldProperties { DefaultValue = 123 }))
                    .AddOrUpdateField(new BooleanField(4, "my-boolean", 
                        new BooleanFieldProperties { DefaultValue = true }))
                    .AddOrUpdateField(new DateTimeField(5, "my-datetime", 
                        new DateTimeFieldProperties { DefaultValue = now }));
            
            var data =
                new ContentData()
                    .AddField("my-string",
                        new ContentFieldData()
                            .AddValue("de", "DE-String"))
                    .AddField("my-number",
                        new ContentFieldData()
                            .AddValue("iv", 456));
            
            schema.Enrich(data, languages);

            Assert.Equal(456, (int)data["my-number"]["iv"]);

            Assert.Equal("DE-String", (string)data["my-string"]["de"]);
            Assert.Equal("EN-String", (string)data["my-string"]["en"]);

            Assert.Equal(now, InstantPattern.General.Parse((string)data["my-datetime"]["iv"]).Value);

            Assert.Equal(true, (bool)data["my-boolean"]["iv"]);
        }
    }
}
