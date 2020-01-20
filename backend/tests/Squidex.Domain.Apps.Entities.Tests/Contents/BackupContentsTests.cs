// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class BackupContentsTests
    {
        private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
        private readonly BackupContents sut;

        public BackupContentsTests()
        {
            sut = new BackupContents(rebuilder);
        }

        [Fact]
        public void Should_provide_name()
        {
            Assert.Equal("Contents", sut.Name);
        }

        [Fact]
        public async Task Should_restore_states_for_all_contents()
        {
            var appId = Guid.NewGuid();

            var schemaId1 = NamedId.Of(Guid.NewGuid(), "my-schema1");
            var schemaId2 = NamedId.Of(Guid.NewGuid(), "my-schema2");

            var contentId1 = Guid.NewGuid();
            var contentId2 = Guid.NewGuid();
            var contentId3 = Guid.NewGuid();

            var context = new RestoreContext(appId, new UserMapping(new RefToken(RefTokenType.Subject, "123")), A.Fake<IBackupReader>());

            await sut.RestoreEventAsync(Envelope.Create(new ContentCreated
            {
                ContentId = contentId1,
                SchemaId = schemaId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new ContentCreated
            {
                ContentId = contentId2,
                SchemaId = schemaId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new ContentCreated
            {
                ContentId = contentId3,
                SchemaId = schemaId2
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new ContentDeleted
            {
                ContentId = contentId2,
                SchemaId = schemaId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new SchemaDeleted
            {
                SchemaId = schemaId2
            }), context);

            var rebuildContents = new HashSet<Guid>();

            var add = new Func<Guid, Task>(id =>
            {
                rebuildContents.Add(id);

                return TaskHelper.Done;
            });

            A.CallTo(() => rebuilder.InsertManyAsync<ContentDomainObject, ContentState>(A<IdSource>.Ignored, A<CancellationToken>.Ignored))
                .Invokes((IdSource source, CancellationToken _) => source(add));

            await sut.RestoreAsync(context);

            Assert.Equal(new HashSet<Guid>
            {
                contentId1,
                contentId2
            }, rebuildContents);
        }
    }
}
