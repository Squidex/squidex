// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent
{
    public class ContentConversionTests
    {
        private readonly Schema schema;

        public ContentConversionTests()
        {
            schema =
                new Schema("my-schema")
                    .AddNumber(1, "field1", Partitioning.Language)
                    .AddNumber(2, "field2", Partitioning.Invariant)
                    .AddNumber(3, "field3", Partitioning.Invariant)
                    .AddAssets(5, "assets1", Partitioning.Invariant)
                    .AddAssets(6, "assets2", Partitioning.Invariant)
                    .AddArray(7, "array", Partitioning.Invariant, h => h
                        .AddNumber(71, "nested1")
                        .AddNumber(72, "nested2"))
                    .AddJson(4, "json", Partitioning.Language)
                    .HideField(2)
                    .HideField(71, 7)
                    .UpdateField(3, f => f.Hide());
        }

        [Fact]
        public void Should_convert_name_to_name()
        {
            var input =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddLocalized("en", "EN"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddInvariant(1))
                    .AddField("invalid",
                        new ContentFieldData()
                            .AddInvariant(2));

            var actual = input.Convert(schema, (data, field) => field.Name == "field2" ? null : data);

            var expected =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddLocalized("en", "EN"));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_be_equal_fields_when_they_have_same_value()
        {
            var lhs =
                new ContentFieldData()
                    .AddInvariant(2);

            var rhs =
                new ContentFieldData()
                    .AddInvariant(2);

            Assert.True(lhs.Equals(rhs));
            Assert.True(lhs.Equals((object)rhs));
            Assert.Equal(lhs.GetHashCode(), rhs.GetHashCode());
        }
    }
}
