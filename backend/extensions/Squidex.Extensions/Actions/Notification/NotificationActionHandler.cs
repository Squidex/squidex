// ==========================================================================
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
using Squidex.Shared.Users;

namespace Squidex.Extensions.Actions.Notification
{
    public sealed class NotificationActionHandler : RuleActionHandler<NotificationAction, CreateComment>
    {
        private const string Description = "Send a Notification";
        private static readonly NamedId<DomainId> NoApp = NamedId.Of(DomainId.Empty, "none");
        private readonly ICommandBus commandBus;
        private readonly IUserResolver userResolver;

        public NotificationActionHandler(RuleEventFormatter formatter, ICommandBus commandBus, IUserResolver userResolver)
            : base(formatter)
        {
            this.commandBus = commandBus;

            this.userResolver = userResolver;
        }

        protected override async Task<(string Description, CreateComment Data)> CreateJobAsync(EnrichedEvent @event, NotificationAction action)
        {
            if (@event is EnrichedUserEventBase userEvent)
            {
                var text = await FormatAsync(action.Text, @event);

                var actor = userEvent.Actor;

                if (!string.IsNullOrEmpty(action.Client))
                {
                    actor = RefToken.Client(action.Client);
                }

                var user = await userResolver.FindByIdOrEmailAsync(action.User);

                if (user == null)
                {
                    throw new InvalidOperationException($"Cannot find user by '{action.User}'");
                }

                var commentsId = DomainId.Create(user.Id);

                var ruleJob = new CreateComment { Actor = actor, CommentsId = commentsId, Text = text };

                if (!string.IsNullOrWhiteSpace(action.Url))
                {
                    var url = await FormatAsync(action.Url, @event);

                    if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
                    {
                        ruleJob.Url = uri;
                    }
                }

                return (Description, ruleJob);
            }

            return ("Ignore", new CreateComment());
        }

        protected override async Task<Result> ExecuteJobAsync(CreateComment job,
            CancellationToken ct = default)
        {
            if (job.CommentsId == default)
            {
                return Result.Ignored();
            }

            var command = job;

            command.AppId = NoApp;
            command.FromRule = true;

            await commandBus.PublishAsync(command);

            return Result.Success($"Notified: {job.Text}");
        }
    }
}
