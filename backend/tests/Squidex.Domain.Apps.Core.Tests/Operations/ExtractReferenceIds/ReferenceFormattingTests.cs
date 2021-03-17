// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ExtractReferenceIds
{
    public class ReferenceFormattingTests
    {
        private readonly LanguagesConfig languages = LanguagesConfig.English.Set(Language.DE);

        [Fact]
        public void Should_format_data_with_reference_fields()
        {
            var data = CreateData();

            var schema =
                new Schema("my-schema")
                    .AddString(1, "ref1", Partitioning.Invariant,
                        new StringFieldProperties())
                    .AddString(2, "ref2", Partitioning.Invariant,
                        new StringFieldProperties())
                    .AddString(3, "non-ref", Partitioning.Invariant)
                    .SetFieldsInReferences("ref1", "ref2");

            var formatted = data.FormatReferences(schema, languages);

            var expected =
                JsonValue.Object()
                    .Add("en", "EN, 12")
                    .Add("de", "DE, 12");

            Assert.Equal(expected, formatted);
        }

        [Fact]
        public void Should_format_data_with_first_field_if_no_reference_field_defined()
        {
            var data = CreateData();

            var schema = CreateNoRefSchema();

            var formatted = data.FormatReferences(schema, languages);

            var expected =
                JsonValue.Object()
                    .Add("en", "EN")
                    .Add("de", "DE");

            Assert.Equal(expected, formatted);
        }

        [Fact]
        public void Should_return_default_value_if_no_value_found()
        {
            var data = new ContentData();

            var schema = CreateNoRefSchema();

            var formatted = data.FormatReferences(schema, languages);

            var expected =
                JsonValue.Object()
                    .Add("en", string.Empty)
                    .Add("de", string.Empty);

            Assert.Equal(expected, formatted);
        }

        private static Schema CreateNoRefSchema()
        {
            return new Schema("my-schema")
                .AddString(1, "ref1", Partitioning.Invariant)
                .AddString(2, "ref2", Partitioning.Invariant)
                .AddString(3, "non-ref", Partitioning.Invariant);
        }

        private static ContentData CreateData()
        {
            return new ContentData()
                .AddField("ref1",
                    new ContentFieldData()
                        .AddLocalized("en", "EN")
                        .AddLocalized("de", "DE"))
                .AddField("ref2",
                    new ContentFieldData()
                        .AddInvariant(12))
                .AddField("non-ref",
                    new ContentFieldData()
                        .AddInvariant("Ignored"));
        }
    }
}
