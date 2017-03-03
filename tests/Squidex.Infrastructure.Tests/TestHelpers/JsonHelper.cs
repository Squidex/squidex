// ==========================================================================
//  JsonHelper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Squidex.Infrastructure.TestHelpers
{
    public static class JsonHelper
    {
        public static void SerializeAndDeserialize<T>(this T value, IContractResolver contractResolver)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                NullValueHandling = NullValueHandling.Include
            };

            var result = JsonConvert.SerializeObject(Tuple.Create(value), serializerSettings);
            var output = JsonConvert.DeserializeObject<Tuple<T>>(result, serializerSettings);

            Assert.Equal(value, output.Item1);
        }

        public static void SerializeAndDeserialize<T>(this T value, JsonConverter converter)
        {
            var serializerSettings = new JsonSerializerSettings();

            serializerSettings.Converters.Add(converter);
            serializerSettings.NullValueHandling = NullValueHandling.Include;

            var result = JsonConvert.SerializeObject(Tuple.Create(value), serializerSettings);
            var output = JsonConvert.DeserializeObject<Tuple<T>>(result, serializerSettings);

            Assert.Equal(value, output.Item1);
        }

        public static void DoesNotDeserialize<T>(string value, JsonConverter converter)
        {
            var serializerSettings = new JsonSerializerSettings();

            serializerSettings.Converters.Add(converter);
            serializerSettings.NullValueHandling = NullValueHandling.Include;

            Assert.ThrowsAny<JsonException>(() => JsonConvert.DeserializeObject<Tuple<T>>($"{{ \"Item1\": \"{value}\" }}", serializerSettings));
        }
    }
}
