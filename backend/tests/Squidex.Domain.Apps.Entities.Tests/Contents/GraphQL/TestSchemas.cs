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

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public static class TestSchemas
    {
        public static readonly NamedId<DomainId> DefaultId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        public static readonly NamedId<DomainId> Ref1Id = NamedId.Of(DomainId.NewGuid(), "my-ref-schema1");
        public static readonly NamedId<DomainId> Ref2Id = NamedId.Of(DomainId.NewGuid(), "my-ref-schema2");

        public static readonly ISchemaEntity Default;
        public static readonly ISchemaEntity Ref1;
        public static readonly ISchemaEntity Ref2;

        static TestSchemas()
        {
            Ref1 = Mocks.Schema(TestApp.DefaultId, Ref1Id,
                new Schema(Ref1Id.Name)
                    .Publish()
                    .AddString(1, "schemaRef1Field", Partitioning.Invariant));

            Ref2 = Mocks.Schema(TestApp.DefaultId, Ref2Id,
                new Schema(Ref2Id.Name)
                    .Publish()
                    .AddString(1, "schemaRef2Field", Partitioning.Invariant));

            var enums = ReadonlyList.Create("EnumA", "EnumB", "EnumC");

            Default = Mocks.Schema(TestApp.DefaultId, DefaultId,
                new Schema(DefaultId.Name)
                    .Publish()
                    .AddJson(1, "my-json", Partitioning.Invariant,
                        new JsonFieldProperties())
                    .AddString(2, "my-string", Partitioning.Invariant,
                        new StringFieldProperties())
                    .AddString(3, "my-string-enum", Partitioning.Invariant,
                        new StringFieldProperties { AllowedValues = enums, CreateEnum = true })
                    .AddString(4, "my-localized-string", Partitioning.Language,
                        new StringFieldProperties())
                    .AddNumber(5, "my-number", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .AddAssets(6, "my-assets", Partitioning.Invariant,
                        new AssetsFieldProperties())
                    .AddBoolean(7, "my-boolean", Partitioning.Invariant,
                        new BooleanFieldProperties())
                    .AddDateTime(8, "my-datetime", Partitioning.Invariant,
                        new DateTimeFieldProperties())
                    .AddReferences(9, "my-references", Partitioning.Invariant,
                        new ReferencesFieldProperties { SchemaId = Ref1Id.Id })
                    .AddReferences(10, "my-union", Partitioning.Invariant,
                        new ReferencesFieldProperties())
                    .AddGeolocation(11, "my-geolocation", Partitioning.Invariant,
                        new GeolocationFieldProperties())
                    .AddComponent(12, "my-component", Partitioning.Invariant,
                        new ComponentFieldProperties { SchemaId = Ref1Id.Id })
                    .AddComponents(13, "my-components", Partitioning.Invariant,
                        new ComponentsFieldProperties { SchemaIds = ReadonlyList.Create(Ref1.Id, Ref2.Id) })
                    .AddTags(14, "my-tags", Partitioning.Invariant,
                        new TagsFieldProperties())
                    .AddTags(15, "my-tags-enum", Partitioning.Invariant,
                        new TagsFieldProperties { AllowedValues = enums, CreateEnum = true })
                    .AddArray(100, "my-array", Partitioning.Invariant, f => f
                        .AddBoolean(121, "nested-boolean",
                            new BooleanFieldProperties())
                        .AddNumber(122, "nested-number",
                            new NumberFieldProperties()))
                    .AddString(16, "my-embeds", Partitioning.Invariant,
                        new StringFieldProperties { IsEmbeddable = true, SchemaIds = ReadonlyList.Create(Ref1.Id, Ref2.Id) })
                    .SetScripts(new SchemaScripts { Query = "<query-script>" }));
        }
    }
}
