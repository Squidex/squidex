// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules.Triggers
{
    public sealed class AssetChangedTriggerHandler : RuleTriggerHandler<AssetChangedTrigger>
    {
        protected override bool Triggers(Envelope<AppEvent> @event, AssetChangedTrigger trigger)
        {
            return @event.Payload is AssetEvent assetEvent && MatchsType(trigger, assetEvent);
        }

        private static bool MatchsType(AssetChangedTrigger trigger, AssetEvent @event)
        {
            return
                trigger.SendCreate && @event is AssetCreated ||
                trigger.SendUpdate && @event is AssetUpdated ||
                trigger.SendDelete && @event is AssetDeleted ||
                trigger.SendRename && @event is AssetRenamed;
        }
    }
}
