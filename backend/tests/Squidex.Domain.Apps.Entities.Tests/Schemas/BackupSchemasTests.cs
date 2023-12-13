// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Schemas;

public class BackupSchemasTests : GivenContext
{
    private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
    private readonly BackupSchemas sut;

    public BackupSchemasTests()
    {
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

        var context = new RestoreContext(AppId.Id, new UserMapping(User), A.Fake<IBackupReader>(), DomainId.NewGuid());

        await sut.RestoreEventAsync(AppEvent(new SchemaCreated
        {
            SchemaId = schemaId1
        }), context, CancellationToken);

        await sut.RestoreEventAsync(AppEvent(new SchemaCreated
        {
            SchemaId = schemaId2
        }), context, CancellationToken);

        await sut.RestoreEventAsync(AppEvent(new SchemaCreated
        {
            SchemaId = schemaId3
        }), context, CancellationToken);

        await sut.RestoreEventAsync(AppEvent(new SchemaDeleted
        {
            SchemaId = schemaId3
        }), context, CancellationToken);

        var rebuildContents = new HashSet<DomainId>();

        A.CallTo(() => rebuilder.InsertManyAsync<SchemaDomainObject, Schema>(A<IEnumerable<DomainId>>._, A<int>._, CancellationToken))
            .Invokes(x => rebuildContents.AddRange(x.GetArgument<IEnumerable<DomainId>>(0)!));

        await sut.RestoreAsync(context, CancellationToken);

        Assert.Equal(
        [
            DomainId.Combine(AppId, schemaId1.Id),
            DomainId.Combine(AppId, schemaId2.Id)
        ], rebuildContents);
    }

    private Envelope<SchemaEvent> AppEvent(SchemaEvent @event)
    {
        @event.AppId = AppId;

        return Envelope.Create(@event).SetAggregateId(DomainId.Combine(AppId, @event.SchemaId.Id));
    }
}
