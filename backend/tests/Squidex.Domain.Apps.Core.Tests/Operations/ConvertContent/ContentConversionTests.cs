// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent
{
    public class ContentConversionTests
    {
        private readonly Schema schema;
        private readonly ResolvedComponents components;

        public ContentConversionTests()
        {
            schema =
                new Schema("my-schema")
                    .AddComponent(1, "component", Partitioning.Invariant)
                    .AddComponents(2, "components", Partitioning.Invariant)
                    .AddAssets(3, "assets1", Partitioning.Invariant)
                    .AddAssets(4, "assets2", Partitioning.Invariant)
                    .AddReferences(5, "references", Partitioning.Invariant)
                    .AddArray(6, "array", Partitioning.Invariant, a => a
                        .AddAssets(31, "nested"));

            components = new ResolvedComponents(new Dictionary<DomainId, Schema>
            {
                [DomainId.Empty] = schema
            });
        }

        [Fact]
        public void Should_apply_value_conversion_on_all_levels()
        {
            var source =
                new ContentData()
                    .AddField("references",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(1, 2)))
                    .AddField("assets1",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(1)))
                    .AddField("array",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Array(
                                    JsonValue.Object()
                                        .Add("nested", JsonValue.Array(1, 2)))))
                    .AddField("component",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("references",
                                        JsonValue.Array(1, 2))
                                    .Add("assets1",
                                        JsonValue.Array(1))
                                    .Add("array",
                                        JsonValue.Array(
                                            JsonValue.Object()
                                                .Add("nested", JsonValue.Array(1, 2))))
                                    .Add(Component.Discriminator, DomainId.Empty)))
                    .AddField("components",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Array(
                                    JsonValue.Object()
                                        .Add("references",
                                            JsonValue.Array(1, 2))
                                        .Add("assets1",
                                            JsonValue.Array(1))
                                        .Add("array",
                                            JsonValue.Array(
                                                JsonValue.Object()
                                                    .Add("nested", JsonValue.Array(1, 2))))
                                        .Add(Component.Discriminator, DomainId.Empty))));

            var expected =
                new ContentData()
                    .AddField("references",
                        new ContentFieldData())
                    .AddField("assets1",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(1)))
                    .AddField("array",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Array(
                                    JsonValue.Object())))
                    .AddField("component",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("assets1",
                                        JsonValue.Array(1))
                                    .Add("array",
                                        JsonValue.Array(
                                            JsonValue.Object()))
                                    .Add(Component.Discriminator, DomainId.Empty)))
                    .AddField("components",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Array(
                                    JsonValue.Object()
                                        .Add("assets1",
                                            JsonValue.Array(1))
                                        .Add("array",
                                            JsonValue.Array(
                                                JsonValue.Object()))
                                        .Add(Component.Discriminator, DomainId.Empty))));

            var converter = new ValueConverter((data, field, parent) => field.Name != "assets1" ? null : data);

            var actual =
                source.Convert(schema,
                    FieldConverters.ForValues(components, converter));

            Assert.Equal(expected, actual);
        }
    }
}
