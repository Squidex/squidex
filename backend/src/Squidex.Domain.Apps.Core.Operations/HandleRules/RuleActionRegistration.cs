// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public sealed class RuleActionRegistration
    {
        public Type ActionType { get; }

        internal RuleActionRegistration(Type actionType)
        {
            Guard.NotNull(actionType, nameof(actionType));

            ActionType = actionType;
        }
    }
}
