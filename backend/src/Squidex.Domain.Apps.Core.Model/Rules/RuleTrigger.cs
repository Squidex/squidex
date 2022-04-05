// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Rules
{
    public abstract record RuleTrigger
    {
        public abstract T Accept<T>(IRuleTriggerVisitor<T> visitor);
    }
}
