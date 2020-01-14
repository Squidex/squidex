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
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

#pragma warning disable RECS0033 // Convert 'if' to '||' expression

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public sealed class RuleRegistry : ITypeProvider
    {
        private const string ActionSuffix = "Action";
        private const string ActionSuffixV2 = "ActionV2";
        private readonly Dictionary<string, RuleActionDefinition> actionTypes = new Dictionary<string, RuleActionDefinition>();

        public IReadOnlyDictionary<string, RuleActionDefinition> Actions
        {
            get { return actionTypes; }
        }

        public RuleRegistry(IEnumerable<RuleActionRegistration>? registrations = null)
        {
            if (registrations != null)
            {
                foreach (var registration in registrations)
                {
                    Add(registration.ActionType);
                }
            }
        }

        public void Add<T>() where T : RuleAction
        {
            Add(typeof(T));
        }

        private void Add(Type actionType)
        {
            var metadata = actionType.GetCustomAttribute<RuleActionAttribute>();

            if (metadata == null)
            {
                return;
            }

            var name = GetActionName(actionType);

            var definition =
                new RuleActionDefinition
                {
                    Type = actionType,
                    Title = metadata.Title,
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

                    if ((GetDataAttribute<RequiredAttribute>(property) != null || (type.IsValueType && !IsNullable(type))) && type != typeof(bool) && type != typeof(bool?))
                    {
                        actionProperty.IsRequired = true;
                    }

                    if (property.GetCustomAttribute<FormattableAttribute>() != null)
                    {
                        actionProperty.IsFormattable = true;
                    }

                    var dataType = GetDataAttribute<DataTypeAttribute>(property)?.DataType;

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

            actionTypes[name] = definition;
        }

        private static T? GetDataAttribute<T>(PropertyInfo property) where T : ValidationAttribute
        {
            var result = property.GetCustomAttribute<T>();

            result?.IsValid(null);

            return result;
        }

        private static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static string GetActionName(Type type)
        {
            return type.TypeName(false, ActionSuffix, ActionSuffixV2);
        }

        public void Map(TypeNameRegistry typeNameRegistry)
        {
            foreach (var actionType in actionTypes.Values)
            {
                typeNameRegistry.Map(actionType.Type, actionType.Type.Name);
            }

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
        }
    }
}
