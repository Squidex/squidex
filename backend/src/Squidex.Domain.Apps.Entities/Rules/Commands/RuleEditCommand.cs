// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Flows.Internal;

namespace Squidex.Domain.Apps.Entities.Rules.Commands;

public abstract class RuleEditCommand : RuleCommand
{
    public string? Name { get; set; }

    public RuleTrigger? Trigger { get; set; }

    public FlowDefinition? Flow { get; set; }

    public bool? IsEnabled { get; set; }
}
