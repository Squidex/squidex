// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules.Triggers
{
    public sealed class AssetChangedTriggerHandler : RuleTriggerHandler<AssetChangedTriggerV2, AssetEvent, EnrichedAssetEvent>
    {
        private readonly IScriptEngine scriptEngine;

        public AssetChangedTriggerHandler(IScriptEngine scriptEngine)
        {
            Guard.NotNull(scriptEngine, nameof(scriptEngine));

            this.scriptEngine = scriptEngine;
        }

        protected override Task<bool> TriggersAsync(EnrichedAssetEvent @event, AssetChangedTriggerV2 trigger)
        {
            return Task.FromResult(string.IsNullOrWhiteSpace(trigger.Condition) || scriptEngine.Evaluate("event", @event, trigger.Condition));
        }
    }
}
