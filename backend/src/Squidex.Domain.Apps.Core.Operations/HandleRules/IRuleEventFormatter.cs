// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public interface IRuleEventFormatter
    {
        (bool Match, string?, int ReplacedLength) Format(EnrichedEvent @event, string text)
        {
            return default;
        }

        (bool Match, ValueTask<string?>) Format(EnrichedEvent @event, object value, string[] path)
        {
            return default;
        }
    }
}