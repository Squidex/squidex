// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public abstract class RuleActionHandler<T> : IRuleActionHandler where T : RuleAction
    {
        Type IRuleActionHandler.ActionType
        {
            get { return typeof(T); }
        }

        (string Description, RuleJobData Data) IRuleActionHandler.CreateJob(Envelope<AppEvent> @event, string eventName, RuleAction action)
        {
            return CreateJob(@event, eventName, (T)action);
        }

        protected abstract (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName, T action);

        public abstract Task<(string Dump, Exception Exception)> ExecuteJobAsync(RuleJobData job);
    }
}
