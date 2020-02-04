// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public interface IEnrichedRuleEntity : IRuleEntity
    {
        int NumSucceeded { get; }

        int NumFailed { get; }

        Instant? LastExecuted { get; set; }
    }
}
