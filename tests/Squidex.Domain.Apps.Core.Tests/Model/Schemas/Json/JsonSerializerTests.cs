// ==========================================================================
//  JsonSerializerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Schemas.Json
{
    public class JsonSerializerTests
    {
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
        private readonly JsonSerializer serializer;
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();

        public JsonSerializerTests()
        {
            serializerSettings.SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry);

            serializerSettings.ContractResolver = new ConverterContractResolver(
                new InstantConverter(),
                new LanguageConverter(),
                new NamedGuidIdConverter(),
                new NamedLongIdConverter(),
                new NamedStringIdConverter(),
                new RefTokenConverter(),
                new SchemaConverter(new FieldRegistry(typeNameRegistry)),
                new StringEnumConverter());

            serializerSettings.TypeNameHandling = TypeNameHandling.Auto;

            serializer = JsonSerializer.Create(serializerSettings);
        }

        [Fact]
        public void Should_serialize_and_deserialize_schema()
        {
            var schemaSource = TestData.MixedSchema();
            var schemaTarget = JToken.FromObject(schemaSource, serializer).ToObject<Schema>(serializer);

            schemaTarget.ShouldBeEquivalentTo(schemaSource);
        }
    }
}
