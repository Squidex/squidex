// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Actions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class RuleActionHandlerAttribute : Attribute
    {
        public Type HandlerType { get; }

        public RuleActionHandlerAttribute(Type handlerType)
        {
            Guard.NotNull(handlerType, nameof(handlerType));

            HandlerType = handlerType;

            if (!typeof(IRuleActionHandler).IsAssignableFrom(handlerType))
            {
                throw new ArgumentException($"Handler type must implement {typeof(IRuleActionHandler)}.", nameof(handlerType));
            }
        }
    }
}
