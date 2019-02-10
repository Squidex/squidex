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
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Actions
{
    public static class RuleElementRegistry
    {
        private const string ActionSuffix = "Action";
        private const string ActionSuffixV2 = "Action";
        private static readonly HashSet<Type> ActionHandlerTypes = new HashSet<Type>();
        private static readonly Dictionary<string, RuleElement> ActionTypes = new Dictionary<string, RuleElement>();

        public static IReadOnlyDictionary<string, RuleElement> Triggers
        {
            get { return TriggerTypes.All; }
        }

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
                var name = GetActionName(actionType);

                var metadata = actionType.GetCustomAttribute<RuleActionAttribute>();

                ActionTypes[name] =
                    new RuleElement
                    {
                        Type = actionType,
                        Display = metadata.Display,
                        Description = metadata.Description,
                        IconColor = metadata.IconColor,
                        IconImage = metadata.IconImage,
                        ReadMore = metadata.ReadMore
                    };

                ActionHandlerTypes.Add(actionType.GetCustomAttribute<RuleActionHandlerAttribute>().HandlerType);
            }
        }

        public static TypeNameRegistry MapRuleActions(this TypeNameRegistry typeNameRegistry)
        {
            foreach (var actionType in ActionTypes.Values)
            {
                typeNameRegistry.Map(actionType.Type, actionType.Type.Name);
            }

            return typeNameRegistry;
        }

        private static string GetActionName(Type type)
        {
            return type.TypeName(false, ActionSuffix, ActionSuffixV2);
        }
    }
}
