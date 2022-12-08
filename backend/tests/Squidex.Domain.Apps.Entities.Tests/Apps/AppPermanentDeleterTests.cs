// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Apps;

public class AppPermanentDeleterTests
{
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly IDeleter deleter1 = A.Fake<IDeleter>();
    private readonly IDeleter deleter2 = A.Fake<IDeleter>();
    private readonly AppPermanentDeleter sut;

    public AppPermanentDeleterTests()
    {
        sut = new AppPermanentDeleter(new[] { deleter1, deleter2 }, domainObjectFactory, TestUtils.TypeRegistry);
    }

    [Fact]
    public void Should_return_assets_filter_for_events_filter()
    {
        IEventConsumer consumer = sut;

        Assert.Equal("^app-", consumer.EventsFilter);
    }

    [Fact]
    public async Task Should_do_nothing_on_clear()
    {
        IEventConsumer consumer = sut;

        await consumer.ClearAsync();
    }

    [Fact]
    public void Should_return_type_name_for_name()
    {
        IEventConsumer consumer = sut;

        Assert.Equal(nameof(AppPermanentDeleter), consumer.Name);
    }

    [Fact]
    public void Should_handle_delete_event()
    {
        var eventType = TestUtils.TypeRegistry.GetName<IEvent, AppDeleted>();

        var storedEvent =
            new StoredEvent("stream", "1", 1,
                new EventData(eventType, new EnvelopeHeaders(), "payload"));

        Assert.True(sut.Handles(storedEvent));
    }

    [Fact]
    public void Should_handle_contributor_event()
    {
        var eventType = TestUtils.TypeRegistry.GetName<IEvent, AppContributorRemoved>();

        var storedEvent =
            new StoredEvent("stream", "1", 1,
                new EventData(eventType, new EnvelopeHeaders(), "payload"));

        Assert.True(sut.Handles(storedEvent));
    }

    [Fact]
    public void Should_not_handle_creation_event()
    {
        var eventType = TestUtils.TypeRegistry.GetName<IEvent, AppCreated>();

        var storedEvent =
            new StoredEvent("stream", "1", 1,
                new EventData(eventType, new EnvelopeHeaders(), "payload"));

        Assert.False(sut.Handles(storedEvent));
    }

    [Fact]
    public async Task Should_call_deleters_when_contributor_removed()
    {
        var app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));

        await sut.On(Envelope.Create(new AppContributorRemoved
        {
            AppId = app.NamedId(), ContributorId = "user1"
        }));

        A.CallTo(() => deleter1.DeleteContributorAsync(app.Id, "user1", default))
            .MustHaveHappened();

        A.CallTo(() => deleter2.DeleteContributorAsync(app.Id, "user1", default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_call_deleters_when_app_deleted()
    {
        var app = new AppDomainObject.State { Id = DomainId.NewGuid(), Name = "my-app" };

        var domainObject = A.Fake<AppDomainObject>();

        A.CallTo(() => domainObject.Snapshot)
            .Returns(app);

        A.CallTo(() => domainObjectFactory.Create<AppDomainObject>(app.Id))
            .Returns(domainObject);

        await sut.On(Envelope.Create(new AppDeleted
        {
            AppId = app.NamedId()
        }));

        A.CallTo(() => deleter1.DeleteAppAsync(app, default))
            .MustHaveHappened();

        A.CallTo(() => deleter2.DeleteAppAsync(app, default))
            .MustHaveHappened();
    }
}
