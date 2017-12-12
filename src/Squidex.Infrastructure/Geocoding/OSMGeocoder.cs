// ==========================================================================
//  OSMGeocoder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Geocoding
{
    public class OSMGeocoder : IGeocoder
    {
        public string Key { get; }

        public object GeocodeAddress(string address)
        {
            throw new InvalidCastException("Latitude and Longitude must be provided.");
        }
    }
}
