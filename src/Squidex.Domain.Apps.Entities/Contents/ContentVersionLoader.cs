// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentVersionLoader : IContentVersionLoader
    {
        private readonly IStore<Guid> store;
        private readonly FieldRegistry registry;

        public ContentVersionLoader(IStore<Guid> store, FieldRegistry registry)
        {
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(registry, nameof(registry));

            this.store = store;

            this.registry = registry;
        }

        public async Task<(ISchemaEntity Schema, IContentEntity Content)> LoadAsync(Guid id, int version)
        {
            var content = new ContentState();

            var persistence = store.WithEventSourcing<ContentGrain, Guid>(id, e =>
            {
                if (content.Version < version)
                {
                    content = content.Apply(e);
                }
            });

            await persistence.ReadAsync();

            if (content.Version != version)
            {
                throw new DomainObjectNotFoundException(id.ToString(), typeof(IContentEntity));
            }

            var (now, then) = await ReadSchema(content.SchemaId.Id, content.LastModified);

            foreach (var key in content.Data.Keys)
            {
                if (IsFieldRemovedOrChanged(then.SchemaDef, now.SchemaDef, key))
                {
                    content.Data.Remove(key);
                }
            }

            return (then, content);
        }

        private static bool IsFieldRemovedOrChanged(Schema schemaThen, Schema schemaNow, string key)
        {
            return
                !schemaThen.FieldsByName.TryGetValue(key, out var fieldThen) ||
                !schemaNow.FieldsByName.TryGetValue(key, out var fieldNow) ||
                fieldThen.GetType() != fieldNow.GetType();
        }

        private async Task<(ISchemaEntity, ISchemaEntity)> ReadSchema(Guid schemaId, Instant lastUpdate)
        {
            var state = new SchemaState();
            var stateAtVersion = (SchemaState)null;

            var persistence = store.WithEventSourcing<SchemaGrain, Guid>(schemaId, e =>
            {
                state = state.Apply(e, registry);

                if (state.LastModified < lastUpdate)
                {
                    stateAtVersion = state;
                }
            });

            await persistence.ReadAsync();

            return (state, stateAtVersion);
        }
    }
}
