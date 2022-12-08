// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Commands;

public sealed class CreateRule : RuleEditCommand
{
    public CreateRule()
    {
        RuleId = DomainId.NewGuid();
    }
}
