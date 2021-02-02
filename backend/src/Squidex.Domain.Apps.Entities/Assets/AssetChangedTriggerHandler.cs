// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
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
        private readonly IAssetRepository assetRepository;

        public override bool CanCreateSnapshotEvents => true;

        public AssetChangedTriggerHandler(
            IScriptEngine scriptEngine,
            IAssetLoader assetLoader,
            IAssetRepository assetRepository)
        {
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(assetLoader, nameof(assetLoader));
            Guard.NotNull(assetRepository, nameof(assetRepository));

            this.scriptEngine = scriptEngine;
            this.assetLoader = assetLoader;
            this.assetRepository = assetRepository;
        }

        public override async IAsyncEnumerable<EnrichedEvent> CreateSnapshotEvents(AssetChangedTriggerV2 trigger, DomainId appId)
        {
            await foreach (var asset in assetRepository.StreamAll(appId))
            {
                var result = new EnrichedAssetEvent
                {
                    Type = EnrichedAssetEventType.Created
                };

                SimpleMapper.Map(asset, result);

                result.Actor = asset.LastModifiedBy;
                result.Name = "AssetCreatedFromSnapshot";

                yield return result;
            }
        }

        protected override async Task<EnrichedAssetEvent?> CreateEnrichedEventAsync(Envelope<AssetEvent> @event)
        {
            if (@event.Payload is AssetMoved)
            {
                return null;
            }

            var result = new EnrichedAssetEvent();

            var asset = await assetLoader.GetAsync(
                @event.Payload.AppId.Id,
                @event.Payload.AssetId,
                @event.Headers.EventStreamNumber());

            if (asset == null)
            {
                throw new DomainObjectNotFoundException(@event.Payload.AssetId.ToString());
            }

            SimpleMapper.Map(asset, result);

            result.AssetType = asset.Type;

            switch (@event.Payload)
            {
                case AssetCreated:
                    result.Type = EnrichedAssetEventType.Created;
                    break;
                case AssetAnnotated:
                    result.Type = EnrichedAssetEventType.Annotated;
                    break;
                case AssetUpdated:
                    result.Type = EnrichedAssetEventType.Updated;
                    break;
                case AssetDeleted:
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

            var vars = new ScriptVars
            {
                ["event"] = @event
            };

            return scriptEngine.Evaluate(vars, trigger.Condition);
        }
    }
}
