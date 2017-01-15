// ==========================================================================
//  EnrichWithSchemaIdProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Events;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;
using Squidex.Write.Schemas;

namespace Squidex.Write
{
    public sealed class EnrichWithSchemaIdProcessor : IEventProcessor
    {
        public Task ProcessEventAsync(Envelope<IEvent> @event, IAggregate aggregate, ICommand command)
        {
            var schemaDomainObject = aggregate as SchemaDomainObject;

            if (schemaDomainObject != null)
            {
                @event.SetSchemaId(aggregate.Id);
            }
            else
            {
                var schemaCommand = command as ISchemaCommand;

                if (schemaCommand != null)
                {
                    @event.SetSchemaId(schemaCommand.SchemaId);
                }
            }

            return TaskHelper.Done;
        }
    }
}
