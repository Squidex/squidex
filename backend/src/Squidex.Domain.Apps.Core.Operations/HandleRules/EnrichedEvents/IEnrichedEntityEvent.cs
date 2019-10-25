// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents
{
    public interface IEnrichedEntityEvent
    {
        Guid Id { get; }
    }
}
