// ==========================================================================
//  MongoRuleRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Domain.Apps.Events.Rules.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Read.MongoDb.Rules
{
    public partial class MongoRuleRepository
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^rules-"; }
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        protected async Task On(RuleCreated @event, EnvelopeHeaders headers)
        {
            await EnsureRulesLoadedAsync();

            await Collection.CreateAsync(@event, headers, w =>
            {
                w.Rule = RuleEventDispatcher.Create(@event);

                inMemoryWebhooks.GetOrAddNew(w.AppId).RemoveAll(x => x.Id == w.Id);
                inMemoryWebhooks.GetOrAddNew(w.AppId).Add(w);
            });
        }

        protected async Task On(RuleUpdated @event, EnvelopeHeaders headers)
        {
            await EnsureRulesLoadedAsync();

            await Collection.UpdateAsync(@event, headers, w =>
            {
                w.Rule.Apply(@event);

                inMemoryWebhooks.GetOrAddNew(w.AppId).RemoveAll(x => x.Id == w.Id);
                inMemoryWebhooks.GetOrAddNew(w.AppId).Add(w);
            });
        }

        protected async Task On(RuleEnabled @event, EnvelopeHeaders headers)
        {
            await EnsureRulesLoadedAsync();

            await Collection.UpdateAsync(@event, headers, w =>
            {
                w.Rule.Apply(@event);

                inMemoryWebhooks.GetOrAddNew(w.AppId).RemoveAll(x => x.Id == w.Id);
                inMemoryWebhooks.GetOrAddNew(w.AppId).Add(w);
            });
        }

        protected async Task On(RuleDisabled @event, EnvelopeHeaders headers)
        {
            await EnsureRulesLoadedAsync();

            await Collection.UpdateAsync(@event, headers, w =>
            {
                w.Rule.Apply(@event);

                inMemoryWebhooks.GetOrAddNew(w.AppId).RemoveAll(x => x.Id == w.Id);
                inMemoryWebhooks.GetOrAddNew(w.AppId).Add(w);
            });
        }

        protected async Task On(RuleDeleted @event, EnvelopeHeaders headers)
        {
            await EnsureRulesLoadedAsync();

            inMemoryWebhooks.GetOrAddNew(@event.AppId.Id).RemoveAll(x => x.Id == @event.RuleId);

            await Collection.DeleteManyAsync(x => x.Id == @event.RuleId);
        }
    }
}
