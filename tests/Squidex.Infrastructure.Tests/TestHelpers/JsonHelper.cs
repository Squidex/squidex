// ==========================================================================
//  JsonHelper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Xunit;

namespace Squidex.Infrastructure.TestHelpers
{
    public static class JsonHelper
    {
        public static void SerializeAndDeserialize<T>(this T value, JsonConverter converter) where T : class
        {
            var serializerSettings = new JsonSerializerSettings();

            serializerSettings.Converters.Add(converter);
            serializerSettings.NullValueHandling = NullValueHandling.Include;

            var result = JsonConvert.SerializeObject(Tuple.Create(value), serializerSettings);
            var output = JsonConvert.DeserializeObject<Tuple<T>>(result, serializerSettings);

            Assert.Equal(value, output.Item1);
        }

        public static void DoesNotDeserialize<T>(string value, JsonConverter converter) where T : class
        {
            var serializerSettings = new JsonSerializerSettings();

            serializerSettings.Converters.Add(converter);
            serializerSettings.NullValueHandling = NullValueHandling.Include;

            Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Tuple<T>>($"{{ \"Item1\": \"{value}\" }}"));
        }
    }
}
