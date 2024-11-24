// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Schemas;

public class SchemaPermanentDeleterTests : GivenContext
{
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly IDeleter deleter1 = A.Fake<IDeleter>();
    private readonly IDeleter deleter2 = A.Fake<IDeleter>();
    private readonly SchemasOptions options = new SchemasOptions();
    private readonly SchemaPermanentDeleter sut;

    public SchemaPermanentDeleterTests()
    {
        sut = new SchemaPermanentDeleter(AppProvider, [deleter1, deleter2], Options.Create(options), domainObjectFactory, TestUtils.TypeRegistry);
    }

    [Fact]
    public void Should_return_assets_filter_for_events_filter()
    {
        Assert.Equal(StreamFilter.Prefix("schema-"), sut.EventsFilter);
    }

    [Fact]
    public async Task Should_do_nothing_on_clear()
    {
        await ((IEventConsumer)sut).ClearAsync();
    }

    [Fact]
    public void Should_return_type_name_for_name()
    {
        Assert.Equal(nameof(SchemaPermanentDeleter), ((IEventConsumer)sut).Name);
    }

    [Fact]
    public async Task Should_handle_delete_event()
    {
        var eventType = TestUtils.TypeRegistry.GetName<IEvent, SchemaDeleted>();

        var storedEvent =
            new StoredEvent("stream", "1", 1,
                new EventData(eventType, [], "payload"));

        Assert.True(await sut.HandlesAsync(storedEvent));
    }

    [Fact]
    public async Task Should_not_handle_creation_event()
    {
        var eventType = TestUtils.TypeRegistry.GetName<IEvent, SchemaCreated>();

        var storedEvent =
            new StoredEvent("stream", "1", 1,
                new EventData(eventType, [], "payload"));

        Assert.False(await sut.HandlesAsync(storedEvent));
    }

    [Fact]
    public async Task Should_call_deleters_when_schema_deleted_and_enabled_globally()
    {
        options.DeletePermanent = true;
        SetupDomainObject();

        await sut.On(Envelope.Create(new SchemaDeleted
        {
            AppId = AppId,
            SchemaId = SchemaId,
            Permanent = false
        }));

        A.CallTo(() => deleter1.DeleteSchemaAsync(App, Schema, default))
            .MustHaveHappened();

        A.CallTo(() => deleter2.DeleteSchemaAsync(App, Schema, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_call_deleters_when_schema_deleted_and_enabled_per_event()
    {
        options.DeletePermanent = false;
        SetupDomainObject();

        await sut.On(Envelope.Create(new SchemaDeleted
        {
            AppId = AppId,
            SchemaId = SchemaId,
            Permanent = true
        }));

        A.CallTo(() => deleter1.DeleteSchemaAsync(App, Schema, default))
            .MustHaveHappened();

        A.CallTo(() => deleter2.DeleteSchemaAsync(App, Schema, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_deleters_when_app_not_deleted_permanently_and_not_enabled_globally()
    {
        options.DeletePermanent = false;

        await sut.On(Envelope.Create(new SchemaDeleted
        {
            AppId = AppId,
            SchemaId = SchemaId,
            Permanent = false
        }));

        A.CallTo(() => deleter1.DeleteSchemaAsync(App, Schema, default))
            .MustNotHaveHappened();

        A.CallTo(() => deleter2.DeleteSchemaAsync(App, Schema, default))
            .MustNotHaveHappened();
    }

    private void SetupDomainObject()
    {
        var domainObject = A.Fake<SchemaDomainObject>();

        A.CallTo(() => domainObject.Snapshot)
            .Returns(Schema);

        A.CallTo(() => domainObjectFactory.Create<SchemaDomainObject>(DomainId.Combine(App.Id, Schema.Id)))
            .Returns(domainObject);
    }
}
