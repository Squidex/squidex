// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class BackupSchemasTests
    {
        private readonly ISchemasIndex index = A.Fake<ISchemasIndex>();
        private readonly BackupSchemas sut;

        public BackupSchemasTests()
        {
            sut = new BackupSchemas(index);
        }

        [Fact]
        public void Should_provide_name()
        {
            Assert.Equal("Schemas", sut.Name);
        }

        [Fact]
        public async Task Should_restore_indices_for_all_non_deleted_schemas()
        {
            var appId = Guid.NewGuid();

            var schemaId1 = NamedId.Of(Guid.NewGuid(), "my-schema1");
            var schemaId2 = NamedId.Of(Guid.NewGuid(), "my-schema2");
            var schemaId3 = NamedId.Of(Guid.NewGuid(), "my-schema3");

            var context = new RestoreContext(appId, new UserMapping(new RefToken(RefTokenType.Subject, "123")), A.Fake<IBackupReader>());

            await sut.RestoreEventAsync(Envelope.Create(new SchemaCreated
            {
                SchemaId = schemaId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new SchemaCreated
            {
                SchemaId = schemaId2
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new SchemaCreated
            {
                SchemaId = schemaId3
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new SchemaDeleted
            {
                SchemaId = schemaId3
            }), context);

            Dictionary<string, Guid>? newIndex = null;

            A.CallTo(() => index.RebuildAsync(appId, A<Dictionary<string, Guid>>.Ignored))
                .Invokes(new Action<Guid, Dictionary<string, Guid>>((_, i) => newIndex = i));

            await sut.RestoreAsync(context);

            Assert.Equal(new Dictionary<string, Guid>
            {
                [schemaId1.Name] = schemaId1.Id,
                [schemaId2.Name] = schemaId2.Id
            }, newIndex);
        }
    }
}
