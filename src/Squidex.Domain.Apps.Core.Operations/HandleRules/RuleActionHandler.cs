// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public abstract class RuleActionHandler<TAction, TData> : IRuleActionHandler where TAction : RuleAction
    {
        Type IRuleActionHandler.ActionType
        {
            get { return typeof(TAction); }
        }

        async Task<(string Description, JObject Data)> IRuleActionHandler.CreateJobAsync(Envelope<AppEvent> @event, string eventName, RuleAction action)
        {
            var (description, data) = await CreateJobAsync(@event, eventName, (TAction)action);

            return (description, JObject.FromObject(data));
        }

        async Task<(string Dump, Exception Exception)> IRuleActionHandler.ExecuteJobAsync(JObject data)
        {
            var typedData = data.ToObject<TData>();

            return await ExecuteJobAsync(typedData);
        }

        protected abstract Task<(string Description, TData Data)> CreateJobAsync(Envelope<AppEvent> @event, string eventName, TAction action);

        protected abstract Task<(string Dump, Exception Exception)> ExecuteJobAsync(TData job);
    }
}
