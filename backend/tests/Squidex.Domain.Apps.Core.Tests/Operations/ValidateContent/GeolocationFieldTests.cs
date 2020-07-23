// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class GeolocationFieldTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new GeolocationFieldProperties());

            Assert.Equal("my-geolocation", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_geolocation_is_valid_null()
        {
            var sut = Field(new GeolocationFieldProperties());

            await sut.ValidateAsync(JsonValue.Null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_geolocation_is_valid()
        {
            var sut = Field(new GeolocationFieldProperties());

            var geolocation = JsonValue.Object()
                .Add("latitude", 0)
                .Add("longitude", 0);

            await sut.ValidateAsync(geolocation, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_geolocation_has_invalid_latitude()
        {
            var sut = Field(new GeolocationFieldProperties { IsRequired = true });

            var geolocation = JsonValue.Object()
                .Add("latitude", 200)
                .Add("longitude", 0);

            await sut.ValidateAsync(geolocation, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Latitude must be between -90 and 90." });
        }

        [Fact]
        public async Task Should_add_error_if_geolocation_has_invalid_longitude()
        {
            var sut = Field(new GeolocationFieldProperties { IsRequired = true });

            var geolocation = JsonValue.Object()
                .Add("latitude", 0)
                .Add("longitude", 200);

            await sut.ValidateAsync(geolocation, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Longitude must be between -180 and 180." });
        }

        [Fact]
        public async Task Should_add_error_if_geolocation_has_too_many_properties()
        {
            var sut = Field(new GeolocationFieldProperties { IsRequired = true });

            var geolocation = JsonValue.Object()
                .Add("invalid", 0)
                .Add("latitude", 0)
                .Add("longitude", 0);

            await sut.ValidateAsync(geolocation, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Geolocation can only have latitude and longitude property." });
        }

        [Fact]
        public async Task Should_add_error_if_geolocation_is_required()
        {
            var sut = Field(new GeolocationFieldProperties { IsRequired = true });

            await sut.ValidateAsync(JsonValue.Null, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        private static RootField<GeolocationFieldProperties> Field(GeolocationFieldProperties properties)
        {
            return Fields.Geolocation(1, "my-geolocation", Partitioning.Invariant, properties);
        }
    }
}
