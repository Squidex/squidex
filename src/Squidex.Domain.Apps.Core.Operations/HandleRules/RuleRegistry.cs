// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public static class RuleRegistry
    {
        public static TypeNameRegistry MapRules(this TypeNameRegistry typeNameRegistry)
        {
            var eventTypes = typeof(EnrichedEvent).Assembly.GetTypes().Where(x => typeof(EnrichedEvent).IsAssignableFrom(x) && !x.IsAbstract);

            var addedTypes = new HashSet<Type>();

            foreach (var type in eventTypes)
            {
                if (addedTypes.Add(type))
                {
                    typeNameRegistry.Map(type, type.Name);
                }
            }

            var triggerTypes = typeof(RuleTrigger).Assembly.GetTypes().Where(x => typeof(RuleTrigger).IsAssignableFrom(x) && !x.IsAbstract);

            foreach (var type in triggerTypes)
            {
                if (addedTypes.Add(type))
                {
                    typeNameRegistry.Map(type, type.Name);
                }
            }

            return typeNameRegistry;
        }
    }
}
