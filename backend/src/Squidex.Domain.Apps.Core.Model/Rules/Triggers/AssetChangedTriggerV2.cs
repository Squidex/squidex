﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules.Triggers;

[TypeName(nameof(AssetChangedTriggerV2))]
public sealed record AssetChangedTriggerV2 : RuleTrigger
{
    public string Condition { get; init; }

    public override T Accept<T, TArgs>(IRuleTriggerVisitor<T, TArgs> visitor, TArgs args)
    {
        return visitor.Visit(this, args);
    }
}
