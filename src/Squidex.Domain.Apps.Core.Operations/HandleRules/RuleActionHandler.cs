// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

#pragma warning disable RECS0083 // Shows NotImplementedException throws in the quick task bar

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public abstract class RuleActionHandler<TAction, TData> : IRuleActionHandler where TAction : RuleAction
    {
        private readonly RuleEventFormatter formatter;

        Type IRuleActionHandler.ActionType
        {
            get { return typeof(TAction); }
        }

        protected RuleActionHandler(RuleEventFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            this.formatter = formatter;
        }

        protected virtual string ToPayloadJson<T>(T @event)
        {
            return formatter.ToPayload(@event).ToString();
        }

        protected virtual string ToEnvelopeJson(EnrichedEvent @event)
        {
            return formatter.ToEnvelope(@event).ToString();
        }

        protected virtual JObject ToPayload<T>(T @event)
        {
            return formatter.ToPayload(@event);
        }

        protected virtual JObject ToEnvelope(EnrichedEvent @event)
        {
            return formatter.ToEnvelope(@event);
        }

        protected string Format(Uri uri, EnrichedEvent @event)
        {
            return formatter.Format(uri.ToString(), @event);
        }

        protected string Format(string text, EnrichedEvent @event)
        {
            return formatter.Format(text, @event);
        }

        async Task<(string Description, JObject Data)> IRuleActionHandler.CreateJobAsync(EnrichedEvent @event, RuleAction action)
        {
            var (description, data) = await CreateJobAsync(@event, (TAction)action);

            return (description, JObject.FromObject(data));
        }

        async Task<(string Dump, Exception Exception)> IRuleActionHandler.ExecuteJobAsync(JObject data)
        {
            var typedData = data.ToObject<TData>();

            return await ExecuteJobAsync(typedData);
        }

        protected virtual Task<(string Description, TData Data)> CreateJobAsync(EnrichedEvent @event, TAction action)
        {
            return Task.FromResult(CreateJob(@event, action));
        }

        protected virtual (string Description, TData Data) CreateJob(EnrichedEvent @event, TAction action)
        {
            throw new NotImplementedException();
        }

        protected abstract Task<(string Dump, Exception Exception)> ExecuteJobAsync(TData job);
    }
}
