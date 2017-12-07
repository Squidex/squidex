// ==========================================================================
//  IGeocoder.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

namespace Squidex.Infrastructure.Geocoding
{
    public interface IGeocoder
    {
        string Key { get; }

        object GeocodeAddress(string address);
    }
}
