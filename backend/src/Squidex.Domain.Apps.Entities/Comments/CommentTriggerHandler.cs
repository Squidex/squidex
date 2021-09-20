// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class CommentTriggerHandler : IRuleTriggerHandler
    {
        private readonly IScriptEngine scriptEngine;
        private readonly IUserResolver userResolver;

        public Type TriggerType => typeof(CommentTrigger);

        public CommentTriggerHandler(IScriptEngine scriptEngine, IUserResolver userResolver)
        {
            this.scriptEngine = scriptEngine;

            this.userResolver = userResolver;
        }

        public bool Handles(AppEvent @event)
        {
            return @event is CommentCreated;
        }

        public async IAsyncEnumerable<EnrichedEvent> CreateEnrichedEventsAsync(Envelope<AppEvent> @event, RuleContext context,
            [EnumeratorCancellation] CancellationToken ct)
        {
            var commentCreated = (CommentCreated)@event.Payload;

            if (commentCreated.Mentions?.Length > 0)
            {
                var users = await userResolver.QueryManyAsync(commentCreated.Mentions, ct);

                if (users.Count > 0)
                {
                    foreach (var user in users.Values)
                    {
                        var enrichedEvent = new EnrichedCommentEvent
                        {
                            MentionedUser = user
                        };

                        enrichedEvent.Name = "UserMentioned";

                        SimpleMapper.Map(commentCreated, enrichedEvent);

                        yield return enrichedEvent;
                    }
                }
            }
        }

        public bool Trigger(EnrichedEvent @event, RuleContext context)
        {
            var trigger = (CommentTrigger)context.Rule.Trigger;

            if (string.IsNullOrWhiteSpace(trigger.Condition))
            {
                return true;
            }

            var vars = new ScriptVars
            {
                ["event"] = @event
            };

            return scriptEngine.Evaluate(vars, trigger.Condition);
        }
    }
}
