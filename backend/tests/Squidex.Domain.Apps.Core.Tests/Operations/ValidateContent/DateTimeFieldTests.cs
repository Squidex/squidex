// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class DateTimeFieldTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new DateTimeFieldProperties());

            Assert.Equal("myDatetime", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_datetime_is_valid()
        {
            var sut = Field(new DateTimeFieldProperties());

            await sut.ValidateAsync(JsonValue.Null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_datetime_is_required()
        {
            var sut = Field(new DateTimeFieldProperties { IsRequired = true });

            await sut.ValidateAsync(JsonValue.Null, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_datetime_is_less_than_min()
        {
            var sut = Field(new DateTimeFieldProperties { MinValue = FutureDays(10) });

            await sut.ValidateAsync(CreateValue(FutureDays(0)), errors);

            errors.Should().BeEquivalentTo(
                new[] { $"Must be greater or equal to {sut.Properties.MinValue}." });
        }

        [Fact]
        public async Task Should_add_error_if_datetime_is_greater_than_max()
        {
            var sut = Field(new DateTimeFieldProperties { MaxValue = FutureDays(10) });

            await sut.ValidateAsync(CreateValue(FutureDays(20)), errors);

            errors.Should().BeEquivalentTo(
                new[] { $"Must be less or equal to {FutureDays(10)}." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_not_valid()
        {
            var sut = Field(new DateTimeFieldProperties());

            await sut.ValidateAsync(JsonValue.Create("Invalid"), errors);

            errors.Should().BeEquivalentTo(
                new[] { "The value string does not match the required number from the format string \"uuuu\". Value being parsed: '^Invalid'. (^ indicates error position.)" });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_another_type()
        {
            var sut = Field(new DateTimeFieldProperties());

            await sut.ValidateAsync(JsonValue.Create(123), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid json type, expected string." });
        }

        private static Instant FutureDays(int days)
        {
            return SystemClock.Instance.GetCurrentInstant().WithoutMs().Plus(Duration.FromDays(days));
        }

        private static IJsonValue CreateValue(Instant v)
        {
            return JsonValue.Create(v);
        }

        private static RootField<DateTimeFieldProperties> Field(DateTimeFieldProperties properties)
        {
            return Fields.DateTime(1, "myDatetime", Partitioning.Invariant, properties);
        }
    }
}
