// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentImporter
    {
        private readonly IStore<Guid> store;
        private readonly IContentWorkflow contentWorkflow;
        private readonly IContextProvider contextProvider;

        public ContentImporter(IStore<Guid> store, IContentWorkflow contentWorkflow, IContextProvider contextProvider)
        {
            Guard.NotNull(store);
            Guard.NotNull(contextProvider);
            Guard.NotNull(contentWorkflow);

            this.store = store;
            this.contentWorkflow = contentWorkflow;
            this.contextProvider = contextProvider;
        }

        public async Task ImportAsync(ISchemaEntity schema, IList<NamedContentData> contents, ContentImportOptions options)
        {
            Guard.NotNull(schema);
            Guard.NotNull(contents);
            Guard.NotNull(options);

            var appId = contextProvider.Context.App.NamedId();

            var schemaId = schema.NamedId();
            var status = Status.Published;

            var actor = contextProvider.Context.User.Token()!;

            if (!options.Publish)
            {
                status = (await contentWorkflow.GetInitialStatusAsync(schema)).Status;
            }

            var events = new Envelope<IEvent>[1];

            foreach (var content in contents)
            {
                var id = Guid.NewGuid();

                var persistence = store.WithSnapshotsAndEventSourcing<ContentState>(typeof(ContentGrain), Guid.NewGuid(), null, null);

                var state = new ContentState { Version = EtagVersion.Empty };

                var @event = Envelope.Create(new ContentCreated
                    {
                        Actor = actor,
                        AppId = appId,
                        ContentId = id,
                        Data = content,
                        SchemaId = schemaId,
                        Status = status
                    })
                    .SetAggregateId(id)
                    .SetAppId(appId.Id);

                state = state.Apply(@event);

                events[0] = @event;

                await persistence.WriteSnapshotAsync(state);
                await persistence.WriteEventsAsync(events);
            }
        }
    }
}
