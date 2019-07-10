using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Avro.Generic;
using Microsoft.Extensions.Hosting;
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
using Squidex.Infrastructure.Log;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class ConsumerService : IHostedService
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly ConsumerOptions options;
        private readonly IKafkaConsumer<GenericRecord> consumer;
        private readonly ICommandBus commandBus;
        private readonly IAppProvider appProvider;
        private readonly IContentQueryService contentQuery;
        private readonly ISemanticLog log;
        private readonly Dictionary<string, Guid> contentIds = new Dictionary<string, Guid>();
        private readonly RefToken actor;
        private readonly ClaimsPrincipal user = CreateUser();
        private IAppEntity app;
        private NamedId<Guid> schemaId;
        private Task consumerTask;

        public ConsumerService(ConsumerOptions options, IKafkaConsumer<GenericRecord> consumer, ICommandBus commandBus, IAppProvider appProvider, IContentQueryService contentQuery, ISemanticLog log)
        {
            this.options = options;
            this.consumer = consumer;
            this.commandBus = commandBus;
            this.appProvider = appProvider;
            this.contentQuery = contentQuery;
            this.log = log;

            actor = new RefToken(RefTokenType.Client, options.ClientName);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            consumerTask = new Task(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var consumed = consumer.Consume(cts.Token);
                        var consumedId = consumed.Message.Key;
                        var consumedFields = consumed.Value.Schema.Fields;

                        await EnsureAppExistsAsync();
                        await CheckOrCreateSchemaAsync(consumedFields);
                        var contentId = await GetContentId(consumedId);
                        var data = CreateContentData(consumed, consumedFields);
                        await CreateOrUpdateContent(consumedId, contentId, data);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, w => w
                            .WriteProperty("action", "createContentConsumedByKafka")
                            .WriteProperty("status", "Failed"));
                    }
                }
            });

            consumerTask.Start();

            return Task.CompletedTask;
        }

        private async Task CreateOrUpdateContent(string consumedId, Guid contentId, NamedContentData data)
        {
            if (contentId != Guid.Empty)
            {
                await commandBus.PublishAsync(new UpdateContent
                {
                    ContentId = contentId,
                    Actor = actor,
                    User = user,
                    Data = data
                });
            }
            else
            {
                var command = new CreateContent
                {
                    AppId = EntityExtensions.NamedId(app),
                    SchemaId = schemaId,
                    Actor = actor,
                    User = user,
                    Publish = true,
                    Data = data
                };

                await commandBus.PublishAsync(command);

                contentIds[consumedId] = command.ContentId;
            }
        }

        private NamedContentData CreateContentData(Confluent.Kafka.ConsumeResult<string, GenericRecord> consumed, List<Avro.Field> consumedFields)
        {
            var data = new NamedContentData();

            if (options.Mapping?.Count > 0)
            {
                foreach (var mapping in options.Mapping)
                {
                    data.AddField(mapping.Key,
                        new ContentFieldData()
                            .AddValue(consumed.Value[mapping.Value]));
                }
            }
            else
            {
                foreach (var field in consumedFields)
                {
                    data.AddField(field.Name,
                        new ContentFieldData()
                            .AddValue(consumed.Value[field.Name]));

                }
            }

            return data;
        }

        private async Task<Guid> GetContentId(string consumedId)
        {
            if (!contentIds.TryGetValue(consumedId, out var contentId))
            {
                var queryContext = QueryContext.Create(app, user, actor.Identifier).WithUnpublished(true);

                var contents = await contentQuery.QueryAsync(queryContext, options.SchemaName, Q.Empty.WithODataQuery($"$filter=data/id/iv eq '{consumedId}'"));
                var contentFound = contents.FirstOrDefault();

                if (contentFound != null)
                {
                    contentId = contentFound.Id;
                    contentIds[consumedId] = contentFound.Id;
                }
            }

            return contentId;
        }

        private async Task EnsureAppExistsAsync()
        {
            if (app == null)
            {
                app = await appProvider.GetAppAsync(options.AppName);
            }

            if (app == null)
            {
                throw new InvalidOperationException($"Cannot find app with name '{options.AppName}'");
            }
        }

        private async Task CheckOrCreateSchemaAsync(List<Avro.Field> fields)
        {
            if (schemaId == null)
            {
                var schema = await appProvider.GetSchemaAsync(app.Id, options.SchemaName);

                schemaId = schema?.NamedId();
            }

            if (schemaId == null)
            {
                var createSchema = new CreateSchema
                {
                    AppId = EntityExtensions.NamedId(app),
                    Actor = actor,
                    User = user,
                    Name = options.SchemaName,
                    Fields = new List<UpsertSchemaField>(),
                    IsPublished = true
                };

                foreach (var field in fields)
                {
                    var type = field.Schema.Tag;

                    var schemaField = new UpsertSchemaField
                    {
                        Name = field.Name
                    };

                    switch (type)
                    {
                        case Avro.Schema.Type.Boolean:
                            schemaField.Properties = new BooleanFieldProperties
                            {
                                IsListField = fields.Count <= 3
                            };
                            break;
                        case Avro.Schema.Type.String:
                            schemaField.Properties = new StringFieldProperties
                            {
                                IsListField = fields.Count <= 3
                            };
                            break;
                        case Avro.Schema.Type.Int:
                        case Avro.Schema.Type.Float:
                        case Avro.Schema.Type.Double:
                            schemaField.Properties = new NumberFieldProperties
                            {
                                IsListField = fields.Count <= 3
                            };
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    createSchema.Fields.Add(schemaField);
                }

                await commandBus.PublishAsync(createSchema);

                schemaId = NamedId.Of(createSchema.SchemaId, options.SchemaName);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cts.Cancel();

            return consumerTask;
        }

        private static ClaimsPrincipal CreateUser()
        {
            var identity = new ClaimsIdentity();

            identity.AddClaim(new Claim(SquidexClaimTypes.Permissions, Permissions.All));

            return new ClaimsPrincipal(identity);
        }
    }
}
