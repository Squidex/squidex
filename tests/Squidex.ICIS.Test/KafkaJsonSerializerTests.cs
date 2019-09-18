using Squidex.ICIS.Kafka;
using System;
using Xunit;

namespace Squidex.ICIS.Test
{
    public class KafkaJsonSerializerTests
    {
        public class JsonObject
        {
            public string Name { get; set; }
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var source = new JsonObject { Name = Guid.NewGuid().ToString() };

            var sut = new KafkaJsonSerializer<JsonObject>(typeof(JsonObject));

            var jsonBytes = sut.Serialize(source, default);
            var jsonResult = sut.Deserialize(jsonBytes, false, default);

            Assert.Equal(source.Name, jsonResult.Name);
        }
    }
}
