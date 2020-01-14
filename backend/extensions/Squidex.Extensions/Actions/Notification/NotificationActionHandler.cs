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
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared.Users;

namespace Squidex.Extensions.Actions.Notification
{
    public sealed class NotificationActionHandler : RuleActionHandler<NotificationAction, NotificationJob>
    {
        private const string Description = "Send a Notification";
        private static readonly NamedId<Guid> NoApp = NamedId.Of(Guid.Empty, "none");
        private readonly ICommandBus commandBus;
        private readonly IUserResolver userResolver;

        public NotificationActionHandler(RuleEventFormatter formatter, ICommandBus commandBus, IUserResolver userResolver)
            : base(formatter)
        {
            Guard.NotNull(commandBus);
            Guard.NotNull(userResolver);

            this.commandBus = commandBus;

            this.userResolver = userResolver;
        }

        protected override async Task<(string Description, NotificationJob Data)> CreateJobAsync(EnrichedEvent @event, NotificationAction action)
        {
            if (@event is EnrichedUserEventBase userEvent)
            {
                var text = Format(action.Text, @event);

                var actor = userEvent.Actor;

                if (!string.IsNullOrEmpty(action.Client))
                {
                    actor = new RefToken(RefTokenType.Client, action.Client);
                }

                var user = await userResolver.FindByIdOrEmailAsync(action.User);

                if (user == null)
                {
                    throw new InvalidOperationException($"Cannot find user by '{action.User}'");
                }

                var ruleJob = new NotificationJob { Actor = actor, CommentsId = user.Id, Text = text };

                if (!string.IsNullOrWhiteSpace(action.Url))
                {
                    var url = Format(action.Url, @event);

                    if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
                    {
                        ruleJob.Url = uri;
                    }
                }

                return (Description, ruleJob);
            }

            return ("Ignore", new NotificationJob());
        }

        protected override async Task<Result> ExecuteJobAsync(NotificationJob job, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(job.CommentsId))
            {
                return Result.Ignored();
            }

            var command = SimpleMapper.Map(job, new CreateComment { AppId = NoApp });

            await commandBus.PublishAsync(command);

            return Result.Success($"Notified: {job.Text}");
        }
    }

    public sealed class NotificationJob
    {
        public RefToken Actor { get; set; }

        public string CommentsId { get; set; }

        public string Text { get; set; }

        public Uri Url { get; set; }
    }
}
