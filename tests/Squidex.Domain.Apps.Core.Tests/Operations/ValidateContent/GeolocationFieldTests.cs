// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class GeolocationFieldTests
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

            await sut.ValidateAsync(CreateValue(JValue.CreateNull()), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_geolocation_is_valid()
        {
            var sut = Field(new GeolocationFieldProperties());

            var geolocation = new JObject(
                new JProperty("latitude", 0),
                new JProperty("longitude", 0));

            await sut.ValidateAsync(CreateValue(geolocation), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_geolocation_has_invalid_latitude()
        {
            var sut = Field(new GeolocationFieldProperties { IsRequired = true });

            var geolocation = new JObject(
                new JProperty("latitude", 200),
                new JProperty("longitude", 0));

            await sut.ValidateAsync(CreateValue(geolocation), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Not a valid value." });
        }

        [Fact]
        public async Task Should_add_errors_if_geolocation_has_invalid_longitude()
        {
            var sut = Field(new GeolocationFieldProperties { IsRequired = true });

            var geolocation = new JObject(
                new JProperty("latitude", 0),
                new JProperty("longitude", 200));

            await sut.ValidateAsync(CreateValue(geolocation), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Not a valid value." });
        }

        [Fact]
        public async Task Should_add_errors_if_geolocation_has_too_many_properties()
        {
            var sut = Field(new GeolocationFieldProperties { IsRequired = true });

            var geolocation = new JObject(
                new JProperty("invalid", 0),
                new JProperty("latitude", 0),
                new JProperty("longitude", 0));

            await sut.ValidateAsync(CreateValue(geolocation), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Not a valid value." });
        }

        [Fact]
        public async Task Should_add_errors_if_geolocation_is_required()
        {
            var sut = Field(new GeolocationFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(JValue.CreateNull()), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        private static JToken CreateValue(JToken v)
        {
            return v;
        }

        private static RootField<GeolocationFieldProperties> Field(GeolocationFieldProperties properties)
        {
            return Fields.Geolocation(1, "my-geolocation", Partitioning.Invariant, properties);
        }
    }
}
