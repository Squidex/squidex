// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.State
{
    public sealed class ContentStateScheduleItem : IContentScheduleItem
    {
        [JsonProperty]
        public Instant ScheduledAt { get; set; }

        [JsonProperty]
        public RefToken ScheduledBy { get; set; }

        [JsonProperty]
        public Status ScheduledTo { get; set; }
    }
}
