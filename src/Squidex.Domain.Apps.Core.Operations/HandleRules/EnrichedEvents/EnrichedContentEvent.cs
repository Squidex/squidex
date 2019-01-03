// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents
{
    public sealed class EnrichedContentEvent : EnrichedSchemaEvent
    {
        public EnrichedContentEventType Type { get; set; }

        public NamedContentData Data { get; set; }

        public Status Status { get; set; }
    }
}
