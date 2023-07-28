// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public static class TestSchemas
{
    public static readonly ISchemaEntity Default;
    public static readonly ISchemaEntity Reference1;
    public static readonly ISchemaEntity Reference2;
    public static readonly ISchemaEntity Component;

    static TestSchemas()
    {
        var enums = ReadonlyList.Create("EnumA", "EnumB", "EnumC");

        var jsonSchema = @"
                type JsonObject {
                    rootString: String
                    rootInt: Int
                    rootFloat: Float
                    rootBoolean: Boolean,
                    rootArray: [String]
                    rootObject: JsonNested
                }

                type JsonNested {
                    nestedString: String
                    nestedInt: Int
                    nestedFloat: Float
                    nestedBoolean: Boolean,
                    nestedArray: [String]
                }";

        Component = Mocks.Schema(TestApp.DefaultId, DomainId.NewGuid(),
            new Schema("my-component", type: SchemaType.Component)
                .AddString(1, "component-field", Partitioning.Invariant));

        Reference1 = Mocks.Schema(TestApp.DefaultId, DomainId.NewGuid(),
            new Schema("my-reference1")
                .Publish()
                .AddString(1, "reference1-field", Partitioning.Invariant));

        Reference2 = Mocks.Schema(TestApp.DefaultId, DomainId.NewGuid(),
            new Schema("my-reference2")
                .Publish()
                .AddString(1, "reference2-field", Partitioning.Invariant));

        Default = Mocks.Schema(TestApp.DefaultId, DomainId.NewGuid(),
            new Schema("my-schema")
                .Publish()
                .AddJson(1, "my-json", Partitioning.Invariant,
                    new JsonFieldProperties())
                .AddJson(2, "my-json2", Partitioning.Invariant,
                    new JsonFieldProperties { GraphQLSchema = jsonSchema })
                .AddString(3, "my-string", Partitioning.Invariant,
                    new StringFieldProperties())
                .AddString(4, "my-string-enum", Partitioning.Invariant,
                    new StringFieldProperties { AllowedValues = enums, CreateEnum = true })
                .AddString(5, "my-localized-string", Partitioning.Language,
                    new StringFieldProperties())
                .AddNumber(6, "my-number", Partitioning.Invariant,
                    new NumberFieldProperties())
                .AddAssets(7, "my-assets", Partitioning.Invariant,
                    new AssetsFieldProperties())
                .AddBoolean(8, "my-boolean", Partitioning.Invariant,
                    new BooleanFieldProperties())
                .AddDateTime(9, "my-datetime", Partitioning.Invariant,
                    new DateTimeFieldProperties())
                .AddReferences(10, "my-references", Partitioning.Invariant,
                    new ReferencesFieldProperties { SchemaId = Reference1.Id })
                .AddReferences(11, "my-union", Partitioning.Invariant,
                    new ReferencesFieldProperties())
                .AddGeolocation(12, "my-geolocation", Partitioning.Invariant,
                    new GeolocationFieldProperties())
                .AddComponent(13, "my-component", Partitioning.Invariant,
                    new ComponentFieldProperties { SchemaId = Component.Id })
                .AddComponents(14, "my-components", Partitioning.Invariant,
                    new ComponentsFieldProperties { SchemaIds = ReadonlyList.Create(Reference1.Id, Reference2.Id, Component.Id) })
                .AddTags(15, "my-tags", Partitioning.Invariant,
                    new TagsFieldProperties())
                .AddTags(16, "my-tags-enum", Partitioning.Invariant,
                    new TagsFieldProperties { AllowedValues = enums, CreateEnum = true })
                .AddArray(100, "my-array", Partitioning.Invariant, f => f
                    .AddBoolean(121, "nested-boolean",
                        new BooleanFieldProperties())
                    .AddNumber(122, "nested-number",
                        new NumberFieldProperties()))
                .AddString(17, "my-embeds", Partitioning.Invariant,
                    new StringFieldProperties { IsEmbeddable = true, SchemaIds = ReadonlyList.Create(Reference1.Id, Reference2.Id) })
                .SetScripts(new SchemaScripts { Query = "<query-script>" }));
    }
}
