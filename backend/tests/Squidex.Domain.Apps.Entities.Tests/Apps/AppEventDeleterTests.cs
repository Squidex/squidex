// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Apps;

public class AppEventDeleterTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IEventStore eventStore = A.Fake<IEventStore>();
    private readonly AppEventDeleter sut;

    public AppEventDeleterTests()
    {
        ct = cts.Token;

        sut = new AppEventDeleter(eventStore);
    }

    [Fact]
    public void Should_run_last()
    {
        var order = sut.Order;

        Assert.Equal(int.MaxValue, order);
    }

    [Fact]
    public async Task Should_remove_events_from_streams()
    {
        var app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));

        await sut.DeleteAppAsync(app, ct);

        A.CallTo(() => eventStore.DeleteAsync($"^[a-zA-Z0-9]-{app.Id}", A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
