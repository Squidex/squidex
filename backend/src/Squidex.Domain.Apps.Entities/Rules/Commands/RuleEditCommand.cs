// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Entities.Rules.Commands;

public abstract class RuleEditCommand : RuleCommand
{
    public string? Name { get; set; }

    public RuleTrigger? Trigger { get; set; }

    public RuleAction? Action { get; set; }

    public bool? IsEnabled { get; set; }
}
