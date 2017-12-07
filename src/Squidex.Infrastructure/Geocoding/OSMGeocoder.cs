// ==========================================================================
//  OSMGeocoder.cs
//  CivicPlus implementation of Squidex Headless CMS
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
