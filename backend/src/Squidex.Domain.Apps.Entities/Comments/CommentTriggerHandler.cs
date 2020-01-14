// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class CommentTriggerHandler : RuleTriggerHandler<CommentTrigger, CommentCreated, EnrichedCommentEvent>
    {
        public override Task<List<EnrichedEvent>> CreateEnrichedEventsAsync(Envelope<AppEvent> @event)
        {
            return base.CreateEnrichedEventsAsync(@event);
        }

        protected override bool Trigger(EnrichedCommentEvent @event, CommentTrigger trigger)
        {
            throw new NotImplementedException();
        }
    }
}
