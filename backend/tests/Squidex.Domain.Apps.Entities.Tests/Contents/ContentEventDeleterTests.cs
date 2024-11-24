// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents;

public class ContentEventDeleterTests : GivenContext
{
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly IEventStore eventStore = A.Fake<IEventStore>();
    private readonly ContentEventDeleter sut;

    public ContentEventDeleterTests()
    {
        sut = new ContentEventDeleter(contentRepository, eventStore);
    }

    [Fact]
    public void Should_run_at_beginning()
    {
        var order = sut.Order;

        Assert.Equal(-1000, order);
    }

    [Fact]
    public async Task Should_do_nothing_when_app_deleted()
    {
        await sut.DeleteAppAsync(App, CancellationToken);

        A.CallTo(eventStore)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_remove_events_from_streams()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();
        var ids = new[] { id1, id2 };

        A.CallTo(() => contentRepository.StreamIds(App.Id, Schema.Id, SearchScope.All, CancellationToken))
            .Returns(ids.ToAsyncEnumerable());

        await sut.DeleteSchemAsync(App, Schema, CancellationToken);

        A.CallTo(() => eventStore.DeleteAsync(
                A<StreamFilter>.That.Matches(x => x.Prefixes!.Contains($"content-{AppId.Id}--{id1}")),
                CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => eventStore.DeleteAsync(
                A<StreamFilter>.That.Matches(x => x.Prefixes!.Contains($"content-{AppId.Id}--{id2}")),
                CancellationToken))
            .MustHaveHappened();
    }
}
