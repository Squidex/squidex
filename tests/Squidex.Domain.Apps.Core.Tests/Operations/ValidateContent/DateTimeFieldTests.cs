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
using Newtonsoft.Json.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class DateTimeFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new DateTimeFieldProperties());

            Assert.Equal("my-datetime", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_datetime_is_valid()
        {
            var sut = Field(new DateTimeFieldProperties());

            await sut.ValidateAsync(CreateValue(null), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_datetime_is_required()
        {
            var sut = Field(new DateTimeFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_errors_if_datetime_is_less_than_min()
        {
            var sut = Field(new DateTimeFieldProperties { MinValue = FutureDays(10) });

            await sut.ValidateAsync(CreateValue(FutureDays(0)), errors);

            errors.ShouldBeEquivalentTo(
                new[] { $"Must be greater or equals than '{FutureDays(10)}'." });
        }

        [Fact]
        public async Task Should_add_errors_if_datetime_is_greater_than_max()
        {
            var sut = Field(new DateTimeFieldProperties { MaxValue = FutureDays(10) });

            await sut.ValidateAsync(CreateValue(FutureDays(20)), errors);

            errors.ShouldBeEquivalentTo(
                new[] { $"Must be less or equals than '{FutureDays(10)}'." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_is_not_valid()
        {
            var sut = Field(new DateTimeFieldProperties());

            await sut.ValidateAsync(CreateValue("Invalid"), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Not a valid value." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_is_another_type()
        {
            var sut = Field(new DateTimeFieldProperties());

            await sut.ValidateAsync(CreateValue(123), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Not a valid value." });
        }

        private static Instant FutureDays(int days)
        {
            return Instant.FromDateTimeUtc(DateTime.UtcNow.Date.AddDays(days));
        }

        private static JValue CreateValue(object v)
        {
            return v is Instant ? new JValue(v.ToString()) : new JValue(v);
        }

        private static RootField<DateTimeFieldProperties> Field(DateTimeFieldProperties properties)
        {
            return Fields.DateTime(1, "my-datetime", Partitioning.Invariant, properties);
        }
    }
}
