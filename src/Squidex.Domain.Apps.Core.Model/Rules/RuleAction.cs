﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules
{
    public abstract class RuleAction : Freezable
    {
        public abstract T Accept<T>(IRuleActionVisitor<T> visitor);
    }
}
