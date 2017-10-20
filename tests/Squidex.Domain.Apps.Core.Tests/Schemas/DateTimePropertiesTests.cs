// ==========================================================================
//  DateTimePropertiesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class DateTimePropertiesTests
    {
        [Fact]
        public void Should_provide_today_default_value()
        {
            var sut = new DateTimeFieldProperties { CalculatedDefaultValue = DateTimeCalculatedDefaultValue.Today };

            Assert.Equal(DateTime.UtcNow.Date.ToString("o"), sut.GetDefaultValue().ToString());
        }

        [Fact]
        public void Should_provide_now_default_value()
        {
            var sut = new DateTimeFieldProperties { CalculatedDefaultValue = DateTimeCalculatedDefaultValue.Now };

            Assert.Equal(DateTime.UtcNow.ToString("o").Substring(0, 16), sut.GetDefaultValue().ToString().Substring(0, 16));
        }

        [Fact]
        public void Should_provide_specific_default_value()
        {
            var sut = new DateTimeFieldProperties { DefaultValue = FutureDays(15) };

            Assert.Equal(FutureDays(15).ToString(), sut.GetDefaultValue());
        }

        private static Instant FutureDays(int days)
        {
            return Instant.FromDateTimeUtc(DateTime.UtcNow.Date.AddDays(days));
        }
    }
}
