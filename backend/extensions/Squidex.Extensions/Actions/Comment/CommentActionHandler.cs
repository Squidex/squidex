﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Extensions.Actions.Comment
{
    public sealed class CommentActionHandler : RuleActionHandler<CommentAction, CommentJob>
    {
        private const string Description = "Send a Comment";
        private readonly ICommandBus commandBus;

        public CommentActionHandler(RuleEventFormatter formatter, ICommandBus commandBus)
            : base(formatter)
        {
            Guard.NotNull(commandBus);

            this.commandBus = commandBus;
        }

        protected override (string Description, CommentJob Data) CreateJob(EnrichedEvent @event, CommentAction action)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                var text = Format(action.Text, @event);

                var actor = contentEvent.Actor;

                if (!string.IsNullOrEmpty(action.Client))
                {
                    actor = new RefToken(RefTokenType.Client, action.Client);
                }

                var ruleJob = new CommentJob
                {
                    AppId = contentEvent.AppId,
                    Actor = actor,
                    CommentsId = contentEvent.Id,
                    Text = text
                };

                return (Description, ruleJob);
            }

            return ("Ignore", new CommentJob());
        }

        protected override async Task<Result> ExecuteJobAsync(CommentJob job, CancellationToken ct = default)
        {
            if (job.CommentsId == Guid.Empty)
            {
                return Result.Ignored();
            }

            var command = SimpleMapper.Map(job, new CreateComment());

            await commandBus.PublishAsync(command);

            return Result.Success($"Commented: {job.Text}");
        }
    }

    public sealed class CommentJob
    {
        public NamedId<Guid> AppId { get; set; }

        public RefToken Actor { get; set; }

        public Guid CommentsId { get; set; }

        public string Text { get; set; }
    }
}
