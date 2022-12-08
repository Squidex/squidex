// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class RuleOptions
{
    public int ExecutionTimeoutInSeconds { get; set; } = 3;

    public TimeSpan RulesCacheDuration { get; set; } = TimeSpan.FromSeconds(10);
}
