// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public static class RuleActionRegistry
    {
        private const string ActionSuffix = "Action";
        private const string ActionSuffixV2 = "ActionV2";
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

            var definition =
                new RuleActionDefinition
                {
                    Type = actionType,
                    Display = metadata.Display,
                    Description = metadata.Description,
                    IconColor = metadata.IconColor,
                    IconImage = metadata.IconImage,
                    ReadMore = metadata.ReadMore
                };

            foreach (var property in actionType.GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                {
                    var actionProperty = new RuleActionProperty { Name = property.Name.ToCamelCase(), Display = property.Name };

                    var display = property.GetCustomAttribute<DisplayAttribute>();

                    if (!string.IsNullOrWhiteSpace(display?.Name))
                    {
                        actionProperty.Display = display.Name;
                    }

                    if (!string.IsNullOrWhiteSpace(display?.Description))
                    {
                        actionProperty.Description = display.Description;
                    }

                    var type = property.PropertyType;

                    if ((property.GetCustomAttribute<RequiredAttribute>() != null || (type.IsValueType && !IsNullable(type))) && type != typeof(bool) && type != typeof(bool?))
                    {
                        actionProperty.IsRequired = true;
                    }

                    if (property.GetCustomAttribute<FormattableAttribute>() != null)
                    {
                        actionProperty.IsFormattable = true;
                    }

                    var dataType = property.GetCustomAttribute<DataTypeAttribute>()?.DataType;

                    if (type == typeof(bool) || type == typeof(bool?))
                    {
                        actionProperty.Editor = RuleActionPropertyEditor.Checkbox;
                    }
                    else if (type == typeof(int) || type == typeof(int?))
                    {
                        actionProperty.Editor = RuleActionPropertyEditor.Number;
                    }
                    else if (dataType == DataType.Url)
                    {
                        actionProperty.Editor = RuleActionPropertyEditor.Url;
                    }
                    else if (dataType == DataType.Password)
                    {
                        actionProperty.Editor = RuleActionPropertyEditor.Password;
                    }
                    else if (dataType == DataType.EmailAddress)
                    {
                        actionProperty.Editor = RuleActionPropertyEditor.Email;
                    }
                    else if (dataType == DataType.MultilineText)
                    {
                        actionProperty.Editor = RuleActionPropertyEditor.TextArea;
                    }
                    else
                    {
                        actionProperty.Editor = RuleActionPropertyEditor.Text;
                    }

                    definition.Properties.Add(actionProperty);
                }
            }

            ActionTypes[name] = definition;

            ActionHandlerTypes.Add(actionType.GetCustomAttribute<RuleActionHandlerAttribute>().HandlerType);
        }

        private static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
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
