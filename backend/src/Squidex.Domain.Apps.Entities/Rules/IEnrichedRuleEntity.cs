// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Domain.Apps.Entities.Rules;

public interface IEnrichedRuleEntity : IRuleEntity
{
    long NumSucceeded { get; }

    long NumFailed { get; }
}
