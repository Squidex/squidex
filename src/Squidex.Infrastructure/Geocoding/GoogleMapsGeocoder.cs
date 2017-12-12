// ==========================================================================
//  GoogleMapsGeocoder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.Geocoding
{
    public class GoogleMapsGeocoder : IGeocoder
    {
        private readonly string geoCodeUrl = "https://maps.googleapis.com/maps/api/geocode/json";
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

        public GoogleMapsGeocoder(string key)
        {
            Key = key;
        }

        public string Key { get; }

        public object GeocodeAddress(string address)
        {
            var requestUrl = $"{geoCodeUrl}?key={Key}&address={address}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            return GetLatLong(request).Result;
        }

        private async Task<object> GetLatLong(HttpRequestMessage request)
        {
            try
            {
                HttpResponseMessage response;
                using (var client = new HttpClient { Timeout = Timeout })
                {
                    response = await client.SendAsync(request);
                }

                var result = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                var innerResults = ((JObject)result["results"].FirstOrDefault());
                var geometry = ((JObject)innerResults["geometry"]);
                var location = ((JObject)geometry["location"]);
                return new { Latitude = location["lat"], Longitude = location["lng"] };
            }
            catch
            {
                throw new InvalidCastException("Latitude and Longitude could not be calculated. Please enter a valid address.");
            }
        }
    }
}
