// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;
using Command = Squidex.Domain.Apps.Entities.Contents.Commands.CreateContent;

namespace Squidex.Extensions.Actions.CreateContent
{
    public sealed class CreateContentActionHandler : RuleActionHandler<CreateContentAction, Command>
    {
        private const string Description = "Create a content";
        private readonly ICommandBus commandBus;
        private readonly IAppProvider appProvider;
        private readonly IJsonSerializer jsonSerializer;

        public CreateContentActionHandler(RuleEventFormatter formatter, IAppProvider appProvider, ICommandBus commandBus, IJsonSerializer jsonSerializer)
            : base(formatter)
        {
            this.appProvider = appProvider;
            this.commandBus = commandBus;
            this.jsonSerializer = jsonSerializer;
        }

        protected override async Task<(string Description, Command Data)> CreateJobAsync(EnrichedEvent @event, CreateContentAction action)
        {
            var ruleJob = new Command
            {
                AppId = @event.AppId
            };

            var schema = await appProvider.GetSchemaAsync(@event.AppId.Id, action.Schema, true);

            if (schema == null)
            {
                throw new InvalidOperationException($"Cannot find schema '{action.Schema}'");
            }

            ruleJob.SchemaId = schema.NamedId();

            var json = await FormatAsync(action.Data, @event);

            ruleJob.Data = jsonSerializer.Deserialize<ContentData>(json);

            if (!string.IsNullOrEmpty(action.Client))
            {
                ruleJob.Actor = RefToken.Client(action.Client);
            }
            else if (@event is EnrichedUserEventBase userEvent)
            {
                ruleJob.Actor = userEvent.Actor;
            }

            if (action.Publish)
            {
                ruleJob.Status = Status.Published;
            }

            return (Description, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(Command job,
            CancellationToken ct = default)
        {
            var command = job;

            command.FromRule = true;

            await commandBus.PublishAsync(command);

            return Result.Success($"Created to: {job.SchemaId.Name}");
        }
    }
}
