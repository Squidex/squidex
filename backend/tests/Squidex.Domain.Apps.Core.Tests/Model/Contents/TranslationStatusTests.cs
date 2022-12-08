// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Model.Contents;

public class TranslationStatusTests
{
    private readonly LanguagesConfig languages = LanguagesConfig.English.Set(Language.DE).Set(Language.IT);

    [Fact]
    public void Should_create_info_for_empty_schema()
    {
        var schema = new Schema("my-schema");

        var actual = TranslationStatus.Create(new ContentData(), schema, languages);

        Assert.Equal(new TranslationStatus
        {
            [Language.EN] = 100,
            [Language.DE] = 100,
            [Language.IT] = 100
        }, actual);
    }

    [Fact]
    public void Should_create_info_for_schema_without_localized_field()
    {
        var schema =
            new Schema("my-schema")
                .AddString(1, "field1", Partitioning.Invariant);

        var actual = TranslationStatus.Create(new ContentData(), schema, languages);

        Assert.Equal(new TranslationStatus
        {
            [Language.EN] = 100,
            [Language.DE] = 100,
            [Language.IT] = 100
        }, actual);
    }

    [Fact]
    public void Should_create_info_for_schema_with_localized_field()
    {
        var schema =
            new Schema("my-schema")
                .AddString(1, "field1", Partitioning.Language);

        var actual = TranslationStatus.Create(new ContentData(), schema, languages);

        Assert.Equal(new TranslationStatus
        {
            [Language.EN] = 0,
            [Language.DE] = 0,
            [Language.IT] = 0
        }, actual);
    }

    [Fact]
    public void Should_create_translation_info()
    {
        var schema =
            new Schema("my-schema")
                .AddString(1, "field1", Partitioning.Language)
                .AddString(2, "field2", Partitioning.Language)
                .AddString(3, "field3", Partitioning.Language)
                .AddString(4, "field4", Partitioning.Invariant);

        var data =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddLocalized(Language.EN, "en")
                        .AddLocalized(Language.DE, "de"))
                .AddField("field2",
                    new ContentFieldData()
                        .AddLocalized(Language.EN, "en")
                        .AddLocalized(Language.DE, "de"))
                .AddField("field3",
                    new ContentFieldData()
                        .AddLocalized(Language.EN, "en"));

        var actual = TranslationStatus.Create(data, schema, languages);

        Assert.Equal(new TranslationStatus
        {
            [Language.EN] = 100,
            [Language.DE] = 67,
            [Language.IT] = 0
        }, actual);
    }
}
