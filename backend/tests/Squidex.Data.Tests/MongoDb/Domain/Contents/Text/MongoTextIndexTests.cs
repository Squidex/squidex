// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1300 // Element should begin with upper-case letter

using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.MongoDb.TestHelpers;

namespace Squidex.MongoDb.Domain.Contents.Text;

[Trait("Category", "Dependencies")]
[Collection("Mongo")]
public class MongoTextIndexTests(MongoFixture fixture) : TextIndexerTestsBase
{
    public override bool SupportsQuerySyntax => false;

    public override bool SupportsGeo => true;

    public override async Task<ITextIndex> CreateSutAsync()
    {
        var sut = new MongoTextIndex(fixture.Database, string.Empty);

        await sut.InitializeAsync(default);
        return sut;
    }

    [Fact]
    public async Task Should_retrieve_all_stopwords_for_english_query()
    {
        await CreateTextAsync(ids1[0], "de", "and und");
        await CreateTextAsync(ids2[0], "en", "and und");

        await SearchText(expected: ids1, text: "de:and");
    }

    [Fact]
    public async Task Should_retrieve_all_stopwords_for_german_query()
    {
        await CreateTextAsync(ids1[0], "de", "and und");
        await CreateTextAsync(ids2[0], "en", "and und");

        await SearchText(expected: ids2, text: "en:und");
    }
}
