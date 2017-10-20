// ==========================================================================
//  JsonSerializerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Immutable;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas.Json
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
            var schema =
                Schema.Create("user", new SchemaProperties { Hints = "The User" })
                    .AddField(new JsonField(1, "my-json", Partitioning.Invariant,
                        new JsonFieldProperties())).HideField(1)
                    .AddField(new AssetsField(2, "my-assets", Partitioning.Invariant,
                        new AssetsFieldProperties())).LockField(2)
                    .AddField(new StringField(3, "my-string1", Partitioning.Language,
                        new StringFieldProperties { Label = "My String1", IsRequired = true, AllowedValues = ImmutableList.Create("a", "b") }))
                    .AddField(new StringField(4, "my-string2", Partitioning.Invariant,
                        new StringFieldProperties { Hints = "My String1" }))
                    .AddField(new NumberField(5, "my-number", Partitioning.Invariant,
                        new NumberFieldProperties { MinValue = 1, MaxValue = 10 }))
                    .AddField(new BooleanField(6, "my-boolean", Partitioning.Invariant,
                        new BooleanFieldProperties())).DisableField(3)
                    .AddField(new DateTimeField(7, "my-datetime", Partitioning.Invariant,
                        new DateTimeFieldProperties { Editor = DateTimeFieldEditor.DateTime }))
                    .AddField(new DateTimeField(8, "my-date", Partitioning.Invariant,
                        new DateTimeFieldProperties { Editor = DateTimeFieldEditor.Date }))
                    .AddField(new ReferencesField(9, "my-references", Partitioning.Invariant,
                        new ReferencesFieldProperties { SchemaId = Guid.NewGuid() }))
                    .AddField(new GeolocationField(10, "my-geolocation", Partitioning.Invariant,
                        new GeolocationFieldProperties()))
                    .AddField(new TagsField(11, "my-tags", Partitioning.Invariant,
                        new TagsFieldProperties()))
                    .Publish();

            var deserialized = JToken.FromObject(schema, serializer).ToObject<Schema>(serializer);

            deserialized.ShouldBeEquivalentTo(schema);
        }
    }
}
