// ==========================================================================
//  ConverterContractResolverTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Json
{
    public class ConverterContractResolverTests
    {
        public class MyClass
        {
            [JsonConverter(typeof(TodayConverter))]
            public Instant MyProperty { get; set; }
        }

        public sealed class TodayConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue("TODAY");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Instant);
            }
        }

        [Fact]
        public void Should_respect_property_converter()
        {
            var value = Instant.FromDateTimeUtc(DateTime.UtcNow.Date);

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new ConverterContractResolver(new InstantConverter())
            };

            var json = JsonConvert.SerializeObject(new MyClass { MyProperty = value }, serializerSettings);

            Assert.Equal(@"{""myProperty"":""TODAY""}", json);
        }

        [Fact]
        public void Should_ignore_other_converters()
        {
            var value = Instant.FromDateTimeUtc(DateTime.UtcNow.Date);

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new ConverterContractResolver(new InstantConverter())
            };

            serializerSettings.Converters.Add(new TodayConverter());

            var result = JsonConvert.SerializeObject(Tuple.Create(value), serializerSettings);
            var output = JsonConvert.DeserializeObject<Tuple<Instant>>(result, serializerSettings);

            Assert.Equal(value, output.Item1);
        }

        [Fact]
        public void Should_serialize_and_deserialize_instant()
        {
            var value = Instant.FromDateTimeUtc(DateTime.UtcNow.Date);

            value.SerializeAndDeserialize(new ConverterContractResolver(new InstantConverter()));
        }
    }
}
