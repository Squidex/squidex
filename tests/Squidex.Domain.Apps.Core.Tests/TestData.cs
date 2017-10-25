// ==========================================================================
//  TestData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Domain.Apps.Core.Apps.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core
{
    public static class TestData
    {
        public static JsonSerializer DefaultSerializer()
        {
            var typeNameRegistry = new TypeNameRegistry();

            var serializerSettings = new JsonSerializerSettings
            {
                SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry),

                ContractResolver = new ConverterContractResolver(
                    new AppClientsConverter(),
                    new AppContributorsConverter(),
                    new InstantConverter(),
                    new LanguageConverter(),
                    new LanguagesConfigConverter(),
                    new NamedGuidIdConverter(),
                    new NamedLongIdConverter(),
                    new NamedStringIdConverter(),
                    new RefTokenConverter(),
                    new SchemaConverter(new FieldRegistry(typeNameRegistry)),
                    new StringEnumConverter()),

                TypeNameHandling = TypeNameHandling.Auto
            };

            return JsonSerializer.Create(serializerSettings);
        }

        public static Schema MixedSchema()
        {
            var inv = Partitioning.Invariant;

            var schema = new Schema("user");

            schema.Publish();
            schema.Update(new SchemaProperties { Hints = "The User" });

            schema.AddField(new JsonField(1, "my-json", inv,
                new JsonFieldProperties()));

            schema.AddField(new AssetsField(2, "my-assets", inv,
                new AssetsFieldProperties()));

            schema.AddField(new StringField(3, "my-string1", inv,
                new StringFieldProperties { Label = "My String1", IsRequired = true, AllowedValues = new[] { "a", "b" } }));

            schema.AddField(new StringField(4, "my-string2", inv,
                new StringFieldProperties { Hints = "My String1" }));

            schema.AddField(new NumberField(5, "my-number", inv,
                new NumberFieldProperties { MinValue = 1, MaxValue = 10 }));

            schema.AddField(new BooleanField(6, "my-boolean", inv,
                new BooleanFieldProperties()));

            schema.AddField(new DateTimeField(7, "my-datetime", inv,
                new DateTimeFieldProperties { Editor = DateTimeFieldEditor.DateTime }));

            schema.AddField(new DateTimeField(8, "my-date", inv,
                new DateTimeFieldProperties { Editor = DateTimeFieldEditor.Date }));

            schema.AddField(new GeolocationField(9, "my-geolocation", inv,
                new GeolocationFieldProperties()));

            schema.AddField(new ReferencesField(10, "my-references", inv,
                new ReferencesFieldProperties()));

            schema.AddField(new TagsField(11, "my-tags", Partitioning.Language,
                new TagsFieldProperties()));

            schema.FieldsById[7].Hide();
            schema.FieldsById[8].Disable();
            schema.FieldsById[9].Lock();

            return schema;
        }
    }
}
