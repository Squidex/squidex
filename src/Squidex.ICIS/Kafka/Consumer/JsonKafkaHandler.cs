using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.ICIS.Kafka.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class JsonKafkaHandler : IKafkaHandler<IRefDataEntity>
    {
        private readonly ICommandBus commandBus;
        private readonly IAppProvider appProvider;
        private readonly IContentQueryService contentQuery;
        private readonly ConcurrentDictionary<string, Guid> contentIds = new ConcurrentDictionary<string, Guid>();
        private readonly ConcurrentDictionary<string, NamedId<Guid>> schemaIds = new ConcurrentDictionary<string, NamedId<Guid>>();

        public JsonKafkaHandler(ICommandBus commandBus, IAppProvider appProvider, IContentQueryService contentQuery)
        {
            this.commandBus = commandBus;
            this.appProvider = appProvider;
            this.contentQuery = contentQuery;
        }

        public async Task HandleAsync(RefToken actor, Context context, string key, IRefDataEntity consumed)
        {
            var schemaId = await GetSchemaAsync(context, consumed);

            var contentId = await GetContentIdAsync(context, consumed);

            await CreateOrUpdateContentAsync(schemaId, actor, context, contentId, consumed);
        }

        private async Task CreateOrUpdateContentAsync(NamedId<Guid> schemaId, RefToken actor, Context context, Guid contentId, IRefDataEntity entity)
        {
            if (contentId != Guid.Empty)
            {
                await PublishAsync(actor, context, new UpdateContent
                {
                    ContentId = contentId,
                    Data = entity.ToData()
                });
            }
            else
            {
                var command = new CreateContent
                {
                    AppId = EntityExtensions.NamedId(context.App),
                    SchemaId = schemaId,
                    Publish = true,
                    Data = entity.ToData()
                };

                await PublishAsync(actor, context, command);

                contentIds[entity.Id] = command.ContentId;
            }
        }

        private async Task<Guid> GetContentIdAsync(Context context, IRefDataEntity entity)
        {
            if (!contentIds.TryGetValue(entity.Id, out var contentId))
            {
                var contents = await contentQuery.QueryAsync(context, entity.Schema, Q.Empty.WithODataQuery($"$filter=data/{entity.IdField}/iv eq '{entity.Id}'"));
                var contentFound = contents.FirstOrDefault();

                if (contentFound != null)
                {
                    contentId = contentFound.Id;
                    contentIds[entity.Id] = contentFound.Id;
                }
            }

            return contentId;
        }

        private async Task<NamedId<Guid>> GetSchemaAsync(Context context, IRefDataEntity entity)
        {
            if (schemaIds.TryGetValue(entity.Schema, out var schemaId))
            {
                return schemaId;
            }

            var schema = await appProvider.GetSchemaAsync(context.App.Id, entity.Schema);

            if (schema == null)
            {
                throw new InvalidOperationException($"Cannot find schema '{entity.Schema}");
            }

            schemaId = schema.NamedId();

            schemaIds[entity.Schema] = schemaId;

            return schemaId;
        }

        private Task PublishAsync(RefToken actor, Context context, SquidexCommand command)
        {
            command.Actor = actor;
            command.User = context.User;

            return commandBus.PublishAsync(command);
        }
    }
}
