﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
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
        private readonly IAssetLoader assetLoader;

        public AssetChangedTriggerHandler(IScriptEngine scriptEngine, IAssetLoader assetLoader)
        {
            Guard.NotNull(scriptEngine);
            Guard.NotNull(assetLoader);

            this.scriptEngine = scriptEngine;

            this.assetLoader = assetLoader;
        }

        protected override async Task<EnrichedAssetEvent?> CreateEnrichedEventAsync(Envelope<AssetEvent> @event)
        {
            if (@event.Payload is AssetMoved)
            {
                return null;
            }

            var result = new EnrichedAssetEvent();

            var asset = await assetLoader.GetAsync(@event.Payload.AssetId, @event.Headers.EventStreamNumber());

            SimpleMapper.Map(asset, result);

            result.AssetType = asset.Type;

            switch (@event.Payload)
            {
                case AssetCreated _:
                    result.Type = EnrichedAssetEventType.Created;
                    break;
                case AssetAnnotated _:
                    result.Type = EnrichedAssetEventType.Annotated;
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
            if (string.IsNullOrWhiteSpace(trigger.Condition))
            {
                return true;
            }

            var context = new ScriptContext
            {
                ["event"] = @event
            };

            return scriptEngine.Evaluate(context, trigger.Condition);
        }
    }
}
