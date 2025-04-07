// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class RulesOptions
{
    public int MaxEnrichedEvents { get; set; } = 500;

    public TimeSpan JobQueryInterval { get; set; } = TimeSpan.FromSeconds(10);

    public TimeSpan RulesCacheDuration { get; set; } = TimeSpan.FromSeconds(10);

    public TimeSpan StaleTime { get; set; } = TimeSpan.FromDays(2);
}
