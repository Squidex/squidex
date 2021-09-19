// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.ObjectPool;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Contents
{
    public static class GeoJsonValue
    {
        public static GeoJsonParseResult TryParse(IJsonValue value, IJsonSerializer serializer, out GeoJSONObject geoJSON)
        {
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(value, nameof(value));

            geoJSON = null!;

            if (value is JsonObject geoObject)
            {
                try
                {
                    using (var stream = DefaultPools.MemoryStream.GetStream())
                    {
                        serializer.Serialize(value, stream, true);

                        stream.Position = 0;

                        geoJSON = serializer.Deserialize<GeoJSONObject>(stream, null, true);

                        return GeoJsonParseResult.Success;
                    }
                }
                catch
                {
                    if (!geoObject.TryGetValue<JsonNumber>("latitude", out var lat) || !lat.Value.IsBetween(-90, 90))
                    {
                        return GeoJsonParseResult.InvalidLatitude;
                    }

                    if (!geoObject.TryGetValue<JsonNumber>("longitude", out var lon) || !lon.Value.IsBetween(-180, 180))
                    {
                        return GeoJsonParseResult.InvalidLongitude;
                    }

                    geoJSON = new Point(new Position(lat.Value, lon.Value));

                    return GeoJsonParseResult.Success;
                }
            }

            return GeoJsonParseResult.InvalidValue;
        }
    }
}
