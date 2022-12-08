// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NetTopologySuite.Geometries;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.ObjectPool;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Contents;

public static class GeoJsonValue
{
    public static GeoJsonParseResult TryParse(JsonValue value, IJsonSerializer serializer, out Geometry? geoJSON)
    {
        Guard.NotNull(serializer);
        Guard.NotNull(value);

        geoJSON = null;

        if (value.Value is JsonObject o)
        {
            if (TryParseGeoJson(o, serializer, out geoJSON))
            {
                return GeoJsonParseResult.Success;
            }

            if (!o.TryGetValue("latitude", out var found) || found.Value is not double lat || !lat.IsBetween(-90, 90))
            {
                return GeoJsonParseResult.InvalidLatitude;
            }

            if (!o.TryGetValue("longitude", out found) || found.Value is not double lon || !lon.IsBetween(-180, 180))
            {
                return GeoJsonParseResult.InvalidLongitude;
            }

            geoJSON = new Point(new Coordinate(lon, lat));

            return GeoJsonParseResult.Success;
        }

        return GeoJsonParseResult.InvalidValue;
    }

    private static bool TryParseGeoJson(JsonObject obj, IJsonSerializer serializer, out Geometry? geoJSON)
    {
        geoJSON = null;

        if (!obj.TryGetValue("type", out var type) || type.Value is not string)
        {
            return false;
        }

        try
        {
            using (var stream = DefaultPools.MemoryStream.GetStream())
            {
                serializer.Serialize(obj, stream, true);

                stream.Position = 0;

                geoJSON = serializer.Deserialize<Geometry>(stream, null, true);

                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}
