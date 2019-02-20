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

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public static class RuleActionRegistry
    {
        private const string ActionSuffix = "Action";
        private const string ActionSuffixV2 = "Action";
        private static readonly HashSet<Type> ActionHandlerTypes = new HashSet<Type>();
        private static readonly Dictionary<string, RuleActionDefinition> ActionTypes = new Dictionary<string, RuleActionDefinition>();

        public static IReadOnlyDictionary<string, RuleActionDefinition> Actions
        {
            get { return ActionTypes; }
        }

        public static IReadOnlyCollection<Type> ActionHandlers
        {
            get { return ActionHandlerTypes; }
        }

        static RuleActionRegistry()
        {
            var actionTypes =
                typeof(RuleActionRegistry).Assembly
                    .GetTypes()
                        .Where(x => typeof(RuleAction).IsAssignableFrom(x))
                        .Where(x => x.GetCustomAttribute<RuleActionAttribute>() != null)
                        .Where(x => x.GetCustomAttribute<RuleActionHandlerAttribute>() != null)
                        .ToList();

            foreach (var actionType in actionTypes)
            {
                Add(actionType);
            }
        }

        public static void Add<T>() where T : RuleAction
        {
            Add(typeof(T));
        }

        private static void Add(Type actionType)
        {
            var metadata = actionType.GetCustomAttribute<RuleActionAttribute>();

            if (metadata == null)
            {
                return;
            }

            var handlerAttribute = actionType.GetCustomAttribute<RuleActionHandlerAttribute>();

            if (handlerAttribute == null)
            {
                return;
            }

            var name = GetActionName(actionType);

            ActionTypes[name] =
                new RuleActionDefinition
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
