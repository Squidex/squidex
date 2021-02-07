﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Domain.Apps.Core.Apps.Json;
using Squidex.Domain.Apps.Core.Contents.Json;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Newtonsoft;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.Reflection;
using Xunit;

namespace Squidex.Domain.Apps.Core.TestHelpers
{
    public static class TestUtils
    {
        public static readonly IJsonSerializer DefaultSerializer = CreateSerializer();

        public static readonly JsonSerializerSettings DefaultSerializerSettings = CreateSerializerSettings();

        public static JsonSerializerSettings CreateSerializerSettings(TypeNameHandling typeNameHandling = TypeNameHandling.Auto,
            JsonConverter? converter = null)
        {
            var typeNameRegistry =
                new TypeNameRegistry()
                    .Map(new FieldRegistry())
                    .Map(new RuleRegistry())
                    .MapUnmapped(typeof(TestUtils).Assembly);

            var serializerSettings = new JsonSerializerSettings
            {
                SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry),

                ContractResolver = new ConverterContractResolver(
                    new AppClientsConverter(),
                    new AppContributorsConverter(),
                    new AppPatternsConverter(),
                    new ClaimsPrincipalConverter(),
                    new ContentFieldDataConverter(),
                    new DomainIdConverter(),
                    new EnvelopeHeadersConverter(),
                    new FilterConverter(),
                    new InstantConverter(),
                    new JsonValueConverter(),
                    new LanguageConverter(),
                    new LanguagesConfigConverter(),
                    new NamedDomainIdConverter(),
                    new NamedGuidIdConverter(),
                    new NamedLongIdConverter(),
                    new NamedStringIdConverter(),
                    new PropertyPathConverter(),
                    new RefTokenConverter(),
                    new RoleConverter(),
                    new RolesConverter(),
                    new RuleConverter(),
                    new SchemaConverter(),
                    new StatusConverter(),
                    new StringEnumConverter(),
                    new WorkflowsConverter(),
                    new WorkflowStepConverter(),
                    new WriteonlyGeoJsonConverter()),

                TypeNameHandling = typeNameHandling
            };

            if (converter != null)
            {
                serializerSettings.Converters.Add(converter);
            }

            return serializerSettings;
        }

        public static IJsonSerializer CreateSerializer(TypeNameHandling typeNameHandling = TypeNameHandling.Auto, JsonConverter? converter = null)
        {
            var serializerSettings = CreateSerializerSettings(typeNameHandling, converter);

            return new NewtonsoftJsonSerializer(serializerSettings);
        }

        public static Schema MixedSchema(bool isSingleton = false)
        {
            var schema = new Schema("user", isSingleton: isSingleton)
                .Publish()
                .AddArray(101, "root-array", Partitioning.Language, f => f
                    .AddAssets(201, "nested-assets")
                    .AddBoolean(202, "nested-boolean")
                    .AddDateTime(203, "nested-datetime")
                    .AddGeolocation(204, "nested-geolocation")
                    .AddJson(205, "nested-json")
                    .AddJson(211, "nested-json2")
                    .AddNumber(206, "nested-number")
                    .AddReferences(207, "nested-references")
                    .AddString(208, "nested-string")
                    .AddTags(209, "nested-tags")
                    .AddUI(210, "nested-ui"))
                .AddAssets(102, "root-assets", Partitioning.Invariant,
                    new AssetsFieldProperties())
                .AddBoolean(103, "root-boolean", Partitioning.Invariant,
                    new BooleanFieldProperties())
                .AddDateTime(104, "root-datetime", Partitioning.Invariant,
                    new DateTimeFieldProperties { Editor = DateTimeFieldEditor.DateTime })
                .AddDateTime(105, "root-date", Partitioning.Invariant,
                    new DateTimeFieldProperties { Editor = DateTimeFieldEditor.Date })
                .AddGeolocation(106, "root-geolocation", Partitioning.Invariant,
                    new GeolocationFieldProperties())
                .AddJson(107, "root-json", Partitioning.Invariant,
                    new JsonFieldProperties())
                .AddNumber(108, "root-number", Partitioning.Invariant,
                    new NumberFieldProperties { MinValue = 1, MaxValue = 10 })
                .AddReferences(109, "root-references", Partitioning.Invariant,
                    new ReferencesFieldProperties())
                .AddString(110, "root-string1", Partitioning.Invariant,
                    new StringFieldProperties { Label = "My String1", IsRequired = true, AllowedValues = ReadOnlyCollection.Create("a", "b") })
                .AddString(111, "root-string2", Partitioning.Invariant,
                    new StringFieldProperties { Hints = "My String1" })
                .AddTags(112, "root-tags", Partitioning.Language,
                    new TagsFieldProperties())
                .AddUI(113, "root-ui", Partitioning.Language,
                    new UIFieldProperties())
                .Update(new SchemaProperties { Hints = "The User" })
                .HideField(104)
                .HideField(211, 101)
                .DisableField(109)
                .DisableField(212, 101)
                .LockField(105);

            return schema;
        }

        public static T SerializeAndDeserialize<T>(this object value)
        {
            return DefaultSerializer.Deserialize<T>(DefaultSerializer.Serialize(value));
        }

        public static T SerializeAndDeserialize<T>(this T value)
        {
            return DefaultSerializer.Deserialize<T>(DefaultSerializer.Serialize(value));
        }

        public static void TestFreeze(IFreezable sut)
        {
            var properties =
                sut.GetType().GetRuntimeProperties()
                    .Where(x =>
                        x.CanWrite &&
                        x.CanRead &&
                        x.Name != "IsFrozen");

            foreach (var property in properties)
            {
                var value =
                    property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

                property.SetValue(sut, value);

                var result = property.GetValue(sut);

                Assert.Equal(value, result);
            }

            sut.Freeze();

            foreach (var property in properties)
            {
                var value =
                    property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

                Assert.Throws<InvalidOperationException>(() =>
                {
                    try
                    {
                        property.SetValue(sut, value);
                    }
                    catch (Exception ex) when (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }
                });
            }
        }
    }
}
