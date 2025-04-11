// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Rules.Deprecated;

[Obsolete("Use Flows")]
public class DeprecatedRule
{
    public string? Name { get; init; }

    public RuleTrigger Trigger { get; init; }

    public RuleAction Action { get; init; }

    public bool IsEnabled { get; init; } = true;
}
