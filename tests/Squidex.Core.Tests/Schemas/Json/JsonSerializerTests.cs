// ==========================================================================
//  JsonSerializerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using FluentAssertions;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Xunit;

namespace Squidex.Core.Schemas.Json
{
    public class JsonSerializerTests
    {
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();
        private readonly SchemaJsonSerializer sut;

        public JsonSerializerTests()
        {
            serializerSettings.TypeNameHandling = TypeNameHandling.Auto;
            serializerSettings.SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry);
            
            sut = new SchemaJsonSerializer(new FieldRegistry(typeNameRegistry), serializerSettings);
        }

        [Fact]
        public void Should_serialize_and_deserialize_schema()
        {
            var schema =
                Schema.Create("my-schema", new SchemaProperties())
                    .AddOrUpdateField(new StringField(1, "field1", 
                        new StringFieldProperties { Label = "Field1", Pattern = "[0-9]{3}" })).DisableField(1)
                    .AddOrUpdateField(new NumberField(2, "field2", 
                        new NumberFieldProperties { Hints = "Hints" }))
                    .AddOrUpdateField(new BooleanField(3, "field3", 
                        new BooleanFieldProperties())).HideField(2)
                    .Publish();

            var deserialized = sut.Deserialize(sut.Serialize(schema));

            deserialized.ShouldBeEquivalentTo(schema);
        }
    }
}
