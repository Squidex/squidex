// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules
{
    public abstract class RuleTrigger : Freezable
    {
        public abstract T Accept<T>(IRuleTriggerVisitor<T> visitor);

        public bool DeepEquals(RuleTrigger action)
        {
            return SimpleEquals.IsEquals(this, action);
        }
    }
}
