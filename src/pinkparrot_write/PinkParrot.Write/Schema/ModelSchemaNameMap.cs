// ==========================================================================
//  ModelSchemaNameMap.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using PinkParrot.Events.Schema;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Infrastructure.Dispatching;
// ReSharper disable InvertIf

namespace PinkParrot.Write.Schema
{
    public class ModelSchemaNameMap : DomainObject
    {
        private readonly Dictionary<string, Guid> schemaIdsByName = new Dictionary<string, Guid>();
        private readonly Dictionary<Guid, string> schemaNamesByIds = new Dictionary<Guid, string>();

        public ModelSchemaNameMap(Guid id, int version) 
            : base(id, version)
        {
        }

        protected void On(ModelSchemaDeleted @event, EnvelopeHeaders headers)
        {
            var aggregateId = headers.AggregateId();

            string oldName;

            if (schemaNamesByIds.TryGetValue(aggregateId, out oldName))
            {
                schemaIdsByName.Remove(oldName);
                schemaNamesByIds.Remove(aggregateId);
            }
        }

        protected void On(ModelSchemaUpdated @event, EnvelopeHeaders headers)
        {
            var aggregateId = headers.AggregateId();

            string oldName;

            if (schemaNamesByIds.TryGetValue(aggregateId, out oldName))
            {
                schemaIdsByName.Remove(oldName);
                schemaIdsByName[@event.NewName] = aggregateId;
            }
        }

        protected void On(ModelSchemaCreated @event, EnvelopeHeaders headers)
        {
            schemaIdsByName[@event.Name] = headers.AggregateId();
        }

        public void Apply(Envelope<IEvent> @event)
        {
            ApplyEvent(@event);
        }

        protected override void ApplyEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload, @event.Headers);
        }
    }
}
