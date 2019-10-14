using Avro.Generic;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avro;
using System.Collections.Concurrent;

namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class AvroKafkaHandler : IKafkaHandler<GenericRecord>
    {
        private readonly AvroConsumerOptions options;
        private readonly ICommandBus commandBus;
        private readonly IAppProvider appProvider;
        private readonly IContentQueryService contentQuery;
        private readonly ConcurrentDictionary<string, Guid> contentIds = new ConcurrentDictionary<string, Guid>();
        private NamedId<Guid> schemaId;
        private ISchemaEntity schema;

        public AvroKafkaHandler(AvroConsumerOptions options, ICommandBus commandBus, IAppProvider appProvider, IContentQueryService contentQuery)
        {
            this.options = options;
            this.commandBus = commandBus;
            this.appProvider = appProvider;
            this.contentQuery = contentQuery;
        }

        public async Task HandleAsync(RefToken actor, Context context, string key, GenericRecord consumed)
        {
            var consumedId = key;
            var consumedFields = consumed.Schema.Fields;

            await CheckOrCreateSchemaAsync(actor, context, consumedFields);

            var contentId = await GetContentIdAsync( context, consumedId);
            var contentData = CreateContentData(consumed, consumedFields);

            await CreateOrUpdateContentAsync(actor, context, consumedId, contentId, contentData);
        }

        private async Task CreateOrUpdateContentAsync(RefToken actor, Context context, string consumedId, Guid contentId, NamedContentData data)
        {
            if (contentId != Guid.Empty)
            {
                await PublishAsync(actor, context, new UpdateContent
                {
                    ContentId = contentId,
                    Data = data
                });
            }
            else
            {
                var command = new CreateContent
                {
                    AppId = EntityExtensions.NamedId(context.App),
                    SchemaId = schemaId,
                    Publish = true,
                    Data = data
                };

                await PublishAsync(actor, context, command);

                contentIds[consumedId] = command.ContentId;
            }
        }

        private NamedContentData CreateContentData(GenericRecord consumed, List<Field> consumedFields)
        {
            var data = new NamedContentData();

            if (options.Mapping?.Count > 0)
            {
                foreach (var mapping in options.Mapping)
                {
                    data.AddField(mapping.Key,
                        new ContentFieldData()
                            .AddValue(consumed[mapping.Value]));
                }
            }
            else
            {
                foreach (var field in consumedFields)
                {
                    data.AddField(field.Name,
                        new ContentFieldData()
                            .AddValue(consumed[field.Name]));

                }
            }

            return data;
        }

        private async Task<Guid> GetContentIdAsync(Context context, string consumedId)
        {
            if (!contentIds.TryGetValue(consumedId, out var contentId))
            {
                var contents = await contentQuery.QueryAsync(context, options.SchemaName, Q.Empty.WithODataQuery($"$filter=data/id/iv eq '{consumedId}'"));
                var contentFound = contents.FirstOrDefault();

                if (contentFound != null)
                {
                    contentId = contentFound.Id;
                    contentIds[consumedId] = contentFound.Id;
                }
            }

            return contentId;
        }

        private async Task CheckOrCreateSchemaAsync(RefToken actor, Context context, List<Field> fields)
        {
            if (schemaId == null)
            {
                schema = await appProvider.GetSchemaAsync(context.App.Id, options.SchemaName);
                schemaId = schema?.NamedId();
            }

            if (schema != null && schemaId != null)
            {
                var isUpdated = false;

                foreach (var field in fields)
                {
                    if (!schema.SchemaDef.FieldsByName.ContainsKey(field.Name))
                    {
                        var schemaField = MapField(field);

                        schemaField.Properties.IsListField = fields.Count <= 3;

                        await PublishAsync(actor, context, new AddField
                        {
                            SchemaId = schemaId.Id,
                            Name = schemaField.Name,
                            Properties = schemaField.Properties,
                            Partitioning = schemaField.Partitioning
                        });

                        isUpdated = true;
                    }
                }

                if (isUpdated)
                {
                    schema = await appProvider.GetSchemaAsync(context.App.Id, options.SchemaName);
                }
            }

            if (schemaId == null)
            {
                var createSchema = new CreateSchema
                {
                    AppId = EntityExtensions.NamedId(context.App),
                    Name = options.SchemaName,
                    Fields = new List<UpsertSchemaField>(),
                    IsPublished = true
                };

                foreach (var field in fields)
                {
                    var schemaField = MapField(field);
                    schemaField.Properties.IsListField = fields.Count <= 3;
                    createSchema.Fields.Add(schemaField);
                }

                await PublishAsync(actor, context, createSchema);

                schemaId = NamedId.Of(createSchema.SchemaId, options.SchemaName);
            }
        }

        private static UpsertSchemaField MapField(Field field)
        {
            var type = field.Schema.Tag;

            var schemaField = new UpsertSchemaField
            {
                Name = field.Name
            };

            switch (type)
            {
                case Avro.Schema.Type.Boolean:
                    schemaField.Properties = new BooleanFieldProperties();
                    break;
                case Avro.Schema.Type.String:
                    schemaField.Properties = new StringFieldProperties();
                    break;
                case Avro.Schema.Type.Int:
                case Avro.Schema.Type.Float:
                case Avro.Schema.Type.Double:
                    schemaField.Properties = new NumberFieldProperties();
                    break;
                default:
                    throw new NotSupportedException();
            }

            return schemaField;
        }

        private Task PublishAsync(RefToken actor, Context context, SquidexCommand command)
        {
            command.Actor = actor;
            command.User = context.User;

            return commandBus.PublishAsync(command);
        }
    }
}