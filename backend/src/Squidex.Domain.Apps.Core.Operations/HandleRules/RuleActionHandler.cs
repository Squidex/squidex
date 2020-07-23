// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
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

        Type IRuleActionHandler.DataType
        {
            get { return typeof(TData); }
        }

        protected RuleActionHandler(RuleEventFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            this.formatter = formatter;
        }

        protected virtual string ToJson<T>(T @event)
        {
            return formatter.ToPayload(@event);
        }

        protected virtual string ToEnvelopeJson(EnrichedEvent @event)
        {
            return formatter.ToEnvelope(@event);
        }

        protected ValueTask<string?> FormatAsync(Uri uri, EnrichedEvent @event)
        {
            return formatter.FormatAsync(uri.ToString(), @event);
        }

        protected ValueTask<string?> FormatAsync(string text, EnrichedEvent @event)
        {
            return formatter.FormatAsync(text, @event);
        }

        async Task<(string Description, object Data)> IRuleActionHandler.CreateJobAsync(EnrichedEvent @event, RuleAction action)
        {
            var (description, data) = await CreateJobAsync(@event, (TAction)action);

            return (description, data!);
        }

        async Task<Result> IRuleActionHandler.ExecuteJobAsync(object data, CancellationToken ct)
        {
            var typedData = (TData)data;

            return await ExecuteJobAsync(typedData, ct);
        }

        protected virtual Task<(string Description, TData Data)> CreateJobAsync(EnrichedEvent @event, TAction action)
        {
            return Task.FromResult(CreateJob(@event, action));
        }

        protected virtual (string Description, TData Data) CreateJob(EnrichedEvent @event, TAction action)
        {
            throw new NotImplementedException();
        }

        protected abstract Task<Result> ExecuteJobAsync(TData job, CancellationToken ct = default);
    }
}
