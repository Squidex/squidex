// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class EventScriptVars : ScriptVars
    {
        public EnrichedEvent Event
        {
            set => SetValue(value);
        }
    }
}
