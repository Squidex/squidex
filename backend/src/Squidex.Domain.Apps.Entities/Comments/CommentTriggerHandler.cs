// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class CommentTriggerHandler : RuleTriggerHandler<CommentTrigger, CommentCreated, EnrichedCommentEvent>
    {
        private static readonly List<EnrichedEvent> EmptyResult = new List<EnrichedEvent>();
        private readonly IScriptEngine scriptEngine;
        private readonly IUserResolver userResolver;

        public CommentTriggerHandler(IScriptEngine scriptEngine, IUserResolver userResolver)
        {
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(userResolver, nameof(userResolver));

            this.scriptEngine = scriptEngine;

            this.userResolver = userResolver;
        }

        public override async Task<List<EnrichedEvent>> CreateEnrichedEventsAsync(Envelope<AppEvent> @event)
        {
            var commentCreated = @event.Payload as CommentCreated;

            if (commentCreated?.Mentions?.Length > 0)
            {
                var users = await userResolver.QueryManyAsync(commentCreated.Mentions);

                if (users.Count > 0)
                {
                    var result = new List<EnrichedEvent>();

                    foreach (var user in users.Values)
                    {
                        var enrichedEvent = new EnrichedCommentEvent
                        {
                            MentionedUser = user
                        };

                        enrichedEvent.Name = "UserMentioned";

                        SimpleMapper.Map(commentCreated, enrichedEvent);

                        result.Add(enrichedEvent);
                    }

                    return result;
                }
            }

            return EmptyResult;
        }

        protected override bool Trigger(EnrichedCommentEvent @event, CommentTrigger trigger)
        {
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
