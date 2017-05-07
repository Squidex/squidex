// ==========================================================================
//  GeolocationFieldTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Squidex.Core.Schemas
{
    public class GeolocationFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = new GeolocationField(1, "my-geolocation", new GeolocationFieldProperties());

            Assert.Equal("my-geolocation", sut.Name);
        }

        [Fact]
        public void Should_clone_object()
        {
            var sut = new GeolocationField(1, "my-geolocation", new GeolocationFieldProperties());

            Assert.NotEqual(sut, sut.Enable());
        }

        [Fact]
        public async Task Should_not_add_error_if_geolocation_is_valid_null()
        {
            var sut = new GeolocationField(1, "my-geolocation", new GeolocationFieldProperties());

            await sut.ValidateAsync(CreateValue(JValue.CreateNull()), false, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_geolocation_is_valid()
        {
            var sut = new GeolocationField(1, "my-geolocation", new GeolocationFieldProperties());

            var geolocation = new JObject(
                new JProperty("latitude", 0),
                new JProperty("longitude", 0));

            await sut.ValidateAsync(CreateValue(geolocation), false, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_geolocation_has_invalid_properties()
        {
            var sut = new GeolocationField(1, "my-geolocation", new GeolocationFieldProperties { IsRequired = true });

            var geolocation = new JObject(
                new JProperty("latitude", 200),
                new JProperty("longitude", 0));

            await sut.ValidateAsync(CreateValue(geolocation), false, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not a valid value" });
        }

        [Fact]
        public async Task Should_add_errors_if_geolocation_has_too_many_properties()
        {
            var sut = new GeolocationField(1, "my-geolocation", new GeolocationFieldProperties { IsRequired = true });

            var geolocation = new JObject(
                new JProperty("invalid", 0),
                new JProperty("latitude", 0),
                new JProperty("longitude", 0));

            await sut.ValidateAsync(CreateValue(geolocation), false, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not a valid value" });
        }

        [Fact]
        public async Task Should_add_errors_if_geolocation_is_required()
        {
            var sut = new GeolocationField(1, "my-geolocation", new GeolocationFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(JValue.CreateNull()), false, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required" });
        }

        private static JToken CreateValue(JToken v)
        {
            return v;
        }
    }
}
