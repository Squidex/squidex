// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1300 // Element should begin with upper-case letter

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

[Trait("Category", "Dependencies")]
public class MongoTextIndexerStateTests : IClassFixture<MongoTextIndexerStateFixture>
{
    public MongoTextIndexerStateFixture _ { get; set; }

    public MongoTextIndexerStateTests(MongoTextIndexerStateFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_add_state()
    {
        var appId = DomainId.NewGuid();
        var id1 = new UniqueContentId(appId, DomainId.NewGuid());
        var id2 = new UniqueContentId(appId, DomainId.NewGuid());
        var id3 = new UniqueContentId(appId, DomainId.NewGuid());

        await _.State.SetAsync(
        [
            new TextContentState { UniqueContentId = id1, State = TextState.Stage0_Draft__Stage1_None },
            new TextContentState { UniqueContentId = id2, State = TextState.Stage0_Published__Stage1_Draft },
            new TextContentState { UniqueContentId = id3, State = TextState.Stage0_Published__Stage1_None }
        ]);

        var actual = await _.State.GetAsync(HashSet.Of(id1, id2));

        actual.Should().BeEquivalentTo(new Dictionary<UniqueContentId, TextContentState>
        {
            [id1] = new TextContentState { UniqueContentId = id1, State = TextState.Stage0_Draft__Stage1_None },
            [id2] = new TextContentState { UniqueContentId = id2, State = TextState.Stage0_Published__Stage1_Draft }
        });
    }

    [Fact]
    public async Task Should_remove_state()
    {
        var id = new UniqueContentId(DomainId.NewGuid(), DomainId.NewGuid());

        await _.State.SetAsync(
        [
            new TextContentState { UniqueContentId = id, State = TextState.Stage0_Draft__Stage1_None }
        ]);

        await _.State.SetAsync(
        [
            new TextContentState { UniqueContentId = id, State = TextState.Deleted }
        ]);

        var actual = await _.State.GetAsync(HashSet.Of(id));

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_remove_by_app_state()
    {
        var appId1 = DomainId.NewGuid();
        var appId2 = DomainId.NewGuid();
        var id1 = new UniqueContentId(appId1, DomainId.NewGuid());
        var id2 = new UniqueContentId(appId1, DomainId.NewGuid());
        var id3 = new UniqueContentId(appId2, DomainId.NewGuid());

        await _.State.SetAsync(
        [
            new TextContentState { UniqueContentId = id1, State = TextState.Stage0_Draft__Stage1_None },
            new TextContentState { UniqueContentId = id2, State = TextState.Stage0_Published__Stage1_Draft },
            new TextContentState { UniqueContentId = id3, State = TextState.Stage0_Published__Stage1_None }
        ]);

        await ((IDeleter)_.State).DeleteAppAsync(new App { Id = appId1 }, default);

        var actual = await _.State.GetAsync(HashSet.Of(id1, id2, id3));

        actual.Should().BeEquivalentTo(new Dictionary<UniqueContentId, TextContentState>
        {
            [id3] = new TextContentState { UniqueContentId = id3, State = TextState.Stage0_Published__Stage1_None }
        });
    }
}
