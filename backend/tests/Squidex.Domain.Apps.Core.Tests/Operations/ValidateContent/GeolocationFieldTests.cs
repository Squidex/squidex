// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent;

public class GeolocationFieldTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = new List<string>();

    [Fact]
    public void Should_instantiate_field()
    {
        var sut = Field(new GeolocationFieldProperties());

        Assert.Equal("myGeolocation", sut.Name);
    }

    [Fact]
    public async Task Should_not_add_error_if_geolocation_is_valid_null()
    {
        var sut = Field(new GeolocationFieldProperties());

        await sut.ValidateAsync(JsonValue.Null, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_geolocation_is_valid_geojson()
    {
        var sut = Field(new GeolocationFieldProperties());

        var geolocation = new JsonObject()
            .Add("coordinates",
                JsonValue.Array(
                    JsonValue.Create(12),
                    JsonValue.Create(45)
                ))
            .Add("type", "Point");

        await sut.ValidateAsync(geolocation, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_geolocation_is_valid()
    {
        var sut = Field(new GeolocationFieldProperties());

        await sut.ValidateAsync(CreateValue(0, 0), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_geolocation_has_invalid_latitude()
    {
        var sut = Field(new GeolocationFieldProperties { IsRequired = true });

        await sut.ValidateAsync(CreateValue(200, 0), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Latitude must be between -90 and 90." });
    }

    [Fact]
    public async Task Should_add_error_if_geolocation_has_invalid_longitude()
    {
        var sut = Field(new GeolocationFieldProperties { IsRequired = true });

        await sut.ValidateAsync(CreateValue(0, 200), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Longitude must be between -180 and 180." });
    }

    [Fact]
    public async Task Should_add_error_if_geolocation_is_required()
    {
        var sut = Field(new GeolocationFieldProperties { IsRequired = true });

        await sut.ValidateAsync(JsonValue.Null, errors);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    private static JsonValue CreateValue(double lat, double lon)
    {
        return new JsonObject().Add("latitude", lat).Add("longitude", lon);
    }

    private static RootField<GeolocationFieldProperties> Field(GeolocationFieldProperties properties)
    {
        return Fields.Geolocation(1, "myGeolocation", Partitioning.Invariant, properties);
    }
}
