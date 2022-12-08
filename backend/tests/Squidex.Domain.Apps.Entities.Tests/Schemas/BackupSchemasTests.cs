// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Schemas;

public class BackupSchemasTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly BackupSchemas sut;

    public BackupSchemasTests()
    {
        ct = cts.Token;

        sut = new BackupSchemas(rebuilder);
    }

    [Fact]
    public void Should_provide_name()
    {
        Assert.Equal("Schemas", sut.Name);
    }

    [Fact]
    public async Task Should_restore_indices_for_all_non_deleted_schemas()
    {
        var schemaId1 = NamedId.Of(DomainId.NewGuid(), "my-schema1");
        var schemaId2 = NamedId.Of(DomainId.NewGuid(), "my-schema2");
        var schemaId3 = NamedId.Of(DomainId.NewGuid(), "my-schema3");

        var context = new RestoreContext(appId.Id, new UserMapping(RefToken.User("123")), A.Fake<IBackupReader>(), DomainId.NewGuid());

        await sut.RestoreEventAsync(AppEvent(new SchemaCreated
        {
            SchemaId = schemaId1
        }), context, ct);

        await sut.RestoreEventAsync(AppEvent(new SchemaCreated
        {
            SchemaId = schemaId2
        }), context, ct);

        await sut.RestoreEventAsync(AppEvent(new SchemaCreated
        {
            SchemaId = schemaId3
        }), context, ct);

        await sut.RestoreEventAsync(AppEvent(new SchemaDeleted
        {
            SchemaId = schemaId3
        }), context, ct);

        var rebuildContents = new HashSet<DomainId>();

        A.CallTo(() => rebuilder.InsertManyAsync<SchemaDomainObject, SchemaDomainObject.State>(A<IEnumerable<DomainId>>._, A<int>._, ct))
            .Invokes(x => rebuildContents.AddRange(x.GetArgument<IEnumerable<DomainId>>(0)!));

        await sut.RestoreAsync(context, ct);

        Assert.Equal(new HashSet<DomainId>
        {
            DomainId.Combine(appId, schemaId1.Id),
            DomainId.Combine(appId, schemaId2.Id)
        }, rebuildContents);
    }

    private Envelope<SchemaEvent> AppEvent(SchemaEvent @event)
    {
        @event.AppId = appId;

        return Envelope.Create(@event).SetAggregateId(DomainId.Combine(appId, @event.SchemaId.Id));
    }
}
