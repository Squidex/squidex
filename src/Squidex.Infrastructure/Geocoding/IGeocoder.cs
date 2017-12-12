// ==========================================================================
//  IGeocoder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.Geocoding
{
    public interface IGeocoder
    {
        string Key { get; }

        object GeocodeAddress(string address);
    }
}
