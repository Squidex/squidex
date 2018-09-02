// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Rules.Actions
{
    public static class RuleElementRegistry
    {
        private const string Suffix = "Action";
        private static readonly HashSet<Type> ActionHandlerTypes = new HashSet<Type>();
        private static readonly Dictionary<string, RuleElement> ActionTypes = new Dictionary<string, RuleElement>();

        public static IReadOnlyDictionary<string, RuleElement> Actions
        {
            get { return ActionTypes; }
        }

        public static IReadOnlyCollection<Type> ActionHandlers
        {
            get { return ActionHandlerTypes; }
        }

        static RuleElementRegistry()
        {
            var actionTypes =
                typeof(RuleElementRegistry).Assembly
                    .GetTypes()
                        .Where(x => typeof(RuleAction).IsAssignableFrom(x))
                        .Where(x => x.GetCustomAttribute<RuleActionAttribute>() != null)
                        .Where(x => x.GetCustomAttribute<RuleActionHandlerAttribute>() != null)
                        .ToList();

            foreach (var actionType in actionTypes)
            {
                var name = actionType.Name;

                if (name.EndsWith(Suffix, StringComparison.Ordinal))
                {
                    name = name.Substring(0, name.Length - Suffix.Length);
                }

                var metadata = actionType.GetCustomAttribute<RuleActionAttribute>();

                ActionTypes[name] =
                    new RuleElement(actionType,
                        metadata.Display,
                        metadata.Description,
                        metadata.Link);

                ActionHandlerTypes.Add(actionType.GetCustomAttribute<RuleActionHandlerAttribute>().HandlerType);
            }
        }

        public static void RegisterTypes(TypeNameRegistry typeNameRegistry)
        {
            foreach (var actionType in ActionTypes.Values)
            {
                typeNameRegistry.Map(actionType.Type, actionType.Type.Name);
            }
        }
    }
}
