// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class BackupContentsTests
    {
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
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
            var schemaId1 = NamedId.Of(DomainId.NewGuid(), "my-schema1");
            var schemaId2 = NamedId.Of(DomainId.NewGuid(), "my-schema2");

            var contentId1 = DomainId.NewGuid();
            var contentId2 = DomainId.NewGuid();
            var contentId3 = DomainId.NewGuid();

            var context = new RestoreContext(appId.Id, new UserMapping(new RefToken(RefTokenType.Subject, "123")), A.Fake<IBackupReader>(), DomainId.NewGuid());

            await sut.RestoreEventAsync(ContentEvent(new ContentCreated
            {
                ContentId = contentId1,
                SchemaId = schemaId1
            }), context);

            await sut.RestoreEventAsync(ContentEvent(new ContentCreated
            {
                ContentId = contentId2,
                SchemaId = schemaId1
            }), context);

            await sut.RestoreEventAsync(ContentEvent(new ContentCreated
            {
                ContentId = contentId3,
                SchemaId = schemaId2
            }), context);

            await sut.RestoreEventAsync(ContentEvent(new ContentDeleted
            {
                ContentId = contentId2,
                SchemaId = schemaId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new SchemaDeleted
            {
                SchemaId = schemaId2
            }), context);

            var rebuildContents = new HashSet<DomainId>();

            A.CallTo(() => rebuilder.InsertManyAsync<ContentDomainObject, ContentState>(A<IEnumerable<DomainId>>._, A<CancellationToken>._))
                .Invokes((IEnumerable<DomainId> source, CancellationToken _) => rebuildContents.AddRange(source));

            await sut.RestoreAsync(context);

            Assert.Equal(new HashSet<DomainId>
            {
                DomainId.Combine(appId.Id, contentId1),
                DomainId.Combine(appId.Id, contentId2)
            }, rebuildContents);
        }

        private Envelope<ContentEvent> ContentEvent(ContentEvent @event)
        {
            @event.AppId = appId;

            var envelope = Envelope.Create(@event);

            envelope.SetAggregateId(DomainId.Combine(appId.Id, @event.ContentId));

            return envelope;
        }
    }
}
