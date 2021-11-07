// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using Microsoft.Spatial;

namespace Squidex.Extensions.Text.Azure
{
    public static class SpatialConverter
    {
        public static GeographyPoint ToSpatialGeometry(this GeoJSONObject obj)
        {
            switch (obj)
            {
                case Point point:
                    return GeographyFactory.Point(point.Coordinates.Latitude, point.Coordinates.Longitude);
                default:
                    return null;
            }
        }
    }
}
