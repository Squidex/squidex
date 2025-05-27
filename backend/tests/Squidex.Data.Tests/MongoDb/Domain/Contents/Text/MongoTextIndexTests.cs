// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.MongoDb.TestHelpers;

namespace Squidex.MongoDb.Domain.Contents.Text;

[Trait("Category", "Dependencies")]
[Collection(MongoFixtureCollection.Name)]
public class MongoTextIndexTests(MongoFixture fixture) : TextIndexerTests
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
        await CreateTextAsync(Ids1[0], "de", "and und");
        await CreateTextAsync(Ids2[0], "en", "and und");

        await SearchText(expected: Ids1, text: "de:and");
    }

    [Fact]
    public async Task Should_retrieve_all_stopwords_for_german_query()
    {
        await CreateTextAsync(Ids1[0], "de", "and und");
        await CreateTextAsync(Ids2[0], "en", "and und");

        await SearchText(expected: Ids2, text: "en:und");
    }
}
