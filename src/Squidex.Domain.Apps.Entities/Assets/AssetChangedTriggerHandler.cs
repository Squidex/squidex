// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetChangedTriggerHandler : RuleTriggerHandler<AssetChangedTriggerV2, AssetEvent, EnrichedAssetEvent>
    {
        private readonly IScriptEngine scriptEngine;
        private readonly IGrainFactory grainFactory;

        public AssetChangedTriggerHandler(IScriptEngine scriptEngine, IGrainFactory grainFactory)
        {
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.scriptEngine = scriptEngine;

            this.grainFactory = grainFactory;
        }

        protected override async Task<EnrichedAssetEvent> CreateEnrichedEventAsync(Envelope<AssetEvent> @event)
        {
            var result = new EnrichedAssetEvent();

            var asset =
                   (await grainFactory
                       .GetGrain<IAssetGrain>(@event.Payload.AssetId)
                       .GetStateAsync(@event.Headers.EventStreamNumber())).Value;

            SimpleMapper.Map(asset, result);

            switch (@event.Payload)
            {
                case AssetCreated _:
                    result.Type = EnrichedAssetEventType.Created;
                    break;
                case AssetRenamed _:
                    result.Type = EnrichedAssetEventType.Renamed;
                    break;
                case AssetUpdated _:
                    result.Type = EnrichedAssetEventType.Updated;
                    break;
                case AssetDeleted _:
                    result.Type = EnrichedAssetEventType.Deleted;
                    break;
            }

            result.Name = $"Asset{result.Type}";

            return result;
        }

        protected override bool Trigger(EnrichedAssetEvent @event, AssetChangedTriggerV2 trigger)
        {
            return string.IsNullOrWhiteSpace(trigger.Condition) || scriptEngine.Evaluate("event", @event, trigger.Condition);
        }
    }
}
