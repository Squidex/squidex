// ==========================================================================
//  AppStateGrainState_Schemas.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Domain.Apps.Events.Schemas.Old;
using Squidex.Domain.Apps.Events.Schemas.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

#pragma warning disable CS0612 // Type or member is obsolete

namespace Squidex.Domain.Apps.Read.State.Grains
{
    public sealed partial class AppStateGrainState
    {
        public void On(SchemaCreated @event, EnvelopeHeaders headers)
        {
            var id = @event.SchemaId.Id;

            Schemas = Schemas.SetItem(id, EntityMapper.Create<JsonSchemaEntity>(@event, headers, s =>
            {
                s.SchemaDef = SchemaEventDispatcher.Create(@event, registry);

                SimpleMapper.Map(@event, s);
            }));
        }

        public void On(SchemaPublished @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event);
            });
        }

        public void On(SchemaUnpublished @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event);
            });
        }

        public void On(ScriptsConfigured @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                SimpleMapper.Map(s, @event);
            });
        }

        public void On(SchemaUpdated @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event);
            });
        }

        public void On(SchemaFieldsReordered @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event);
            });
        }

        public void On(FieldAdded @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event, registry);
            });
        }

        public void On(FieldUpdated @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event);
            });
        }

        public void On(FieldLocked @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event);
            });
        }

        public void On(FieldDisabled @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event);
            });
        }

        public void On(FieldEnabled @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event);
            });
        }

        public void On(FieldHidden @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event);
            });
        }

        public void On(FieldShown @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event);
            });
        }

        public void On(FieldDeleted @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers, s =>
            {
                s.SchemaDef = s.SchemaDef.Apply(@event);
            });
        }

        public void On(WebhookAdded @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers);
        }

        public void On(WebhookDeleted @event, EnvelopeHeaders headers)
        {
            UpdateSchema(@event, headers);
        }

        public void On(SchemaDeleted @event, EnvelopeHeaders headers)
        {
            Schemas = Schemas.Remove(@event.SchemaId.Id);
        }

        private void UpdateSchema(SchemaEvent @event, EnvelopeHeaders headers, Action<JsonSchemaEntity> updater = null)
        {
            var id = @event.SchemaId.Id;

            Schemas = Schemas.SetItem(id, x => x.Clone().Update(@event, headers, updater));
        }
    }
}
