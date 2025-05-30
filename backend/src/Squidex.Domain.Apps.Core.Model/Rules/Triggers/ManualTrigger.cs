﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules.Triggers;

[TypeName(nameof(ManualTrigger))]
public sealed record ManualTrigger : RuleTrigger
{
    public override T Accept<T, TArgs>(IRuleTriggerVisitor<T, TArgs> visitor, TArgs args)
    {
        return visitor.Visit(this, args);
    }
}
