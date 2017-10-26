// ==========================================================================
//  RuleEventDispatcher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events.Webhooks;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Events.Rules.Utils
{
    public static class RuleEventDispatcher
    {
        public static Rule Create(RuleCreated @event)
        {
            return new Rule(@event.Trigger, @event.Action);
        }

        public static Rule Create(WebhookCreated @event)
        {
            return new Rule(CreateTrigger(@event), CreateAction(@event));
        }

        public static void Apply(this Rule rule, WebhookUpdated @event)
        {
            rule.Update(CreateTrigger(@event));

            if (rule.Action is WebhookAction webhookAction)
            {
                webhookAction.Url = @event.Url;
            }
        }

        public static void Apply(this Rule rule, RuleUpdated @event)
        {
            if (@event.Trigger != null)
            {
                rule.Update(@event.Trigger);
            }

            if (@event.Action != null)
            {
                rule.Update(@event.Action);
            }
        }

        public static void Apply(this Rule rule, RuleEnabled @event)
        {
            rule.Enable();
        }

        public static void Apply(this Rule rule, RuleDisabled @event)
        {
            rule.Disable();
        }

        private static WebhookAction CreateAction(WebhookCreated @event)
        {
            var action = new WebhookAction { Url = @event.Url, SharedSecret = @event.SharedSecret };

            return action;
        }

        private static ContentChangedTrigger CreateTrigger(WebhookEditEvent @event)
        {
            var trigger = new ContentChangedTrigger
            {
                Schemas = @event.Schemas.Select(x => SimpleMapper.Map(x, new ContentChangedTriggerSchema())).ToList()
            };

            return trigger;
        }
    }
}
