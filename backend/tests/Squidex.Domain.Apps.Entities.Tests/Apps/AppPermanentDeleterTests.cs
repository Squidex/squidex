// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps;

public class AppPermanentDeleterTests
{
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly IDeleter deleter1 = A.Fake<IDeleter>();
    private readonly IDeleter deleter2 = A.Fake<IDeleter>();
    private readonly TypeNameRegistry typeNameRegistry;
    private readonly AppPermanentDeleter sut;

    public AppPermanentDeleterTests()
    {
        typeNameRegistry =
            new TypeNameRegistry()
                .Map(typeof(AppCreated))
                .Map(typeof(AppContributorRemoved))
                .Map(typeof(AppDeleted));

        sut = new AppPermanentDeleter(new[] { deleter1, deleter2 }, domainObjectFactory, typeNameRegistry);
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
        var storedEvent =
            new StoredEvent("stream", "1", 1,
                new EventData(typeNameRegistry.GetName<AppDeleted>(), new EnvelopeHeaders(), "payload"));

        Assert.True(sut.Handles(storedEvent));
    }

    [Fact]
    public void Should_handle_contributor_event()
    {
        var storedEvent =
            new StoredEvent("stream", "1", 1,
                new EventData(typeNameRegistry.GetName<AppContributorRemoved>(), new EnvelopeHeaders(), "payload"));

        Assert.True(sut.Handles(storedEvent));
    }

    [Fact]
    public void Should_not_handle_creation_event()
    {
        var storedEvent =
            new StoredEvent("stream", "1", 1,
                new EventData(typeNameRegistry.GetName<AppCreated>(), new EnvelopeHeaders(), "payload"));

        Assert.False(sut.Handles(storedEvent));
    }

    [Fact]
    public async Task Should_call_deleters_when_contributor_removed()
    {
        var app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));

        await sut.On(Envelope.Create(new AppContributorRemoved
        {
            AppId = app.NamedId(),
            ContributorId = "user1"
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
