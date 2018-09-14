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
        private const string Suffix = "Action";
        private static readonly HashSet<Type> ActionHandlerTypes = new HashSet<Type>();
        private static readonly Dictionary<string, RuleElement> ActionTypes = new Dictionary<string, RuleElement>();
        private static readonly Dictionary<string, RuleElement> TriggerTypes = new Dictionary<string, RuleElement>
        {
            ["ContentChanged"] = new RuleElement
            {
                IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 28 28'><path d='M21.875 28H6.125A6.087 6.087 0 0 1 0 21.875V6.125A6.087 6.087 0 0 1 6.125 0h15.75A6.087 6.087 0 0 1 28 6.125v15.75A6.088 6.088 0 0 1 21.875 28zM6.125 1.75A4.333 4.333 0 0 0 1.75 6.125v15.75a4.333 4.333 0 0 0 4.375 4.375h15.75a4.333 4.333 0 0 0 4.375-4.375V6.125a4.333 4.333 0 0 0-4.375-4.375H6.125z'/><path d='M13.125 12.25H7.35c-1.575 0-2.888-1.313-2.888-2.888V7.349c0-1.575 1.313-2.888 2.888-2.888h5.775c1.575 0 2.887 1.313 2.887 2.888v2.013c0 1.575-1.312 2.888-2.887 2.888zM7.35 6.212c-.613 0-1.138.525-1.138 1.138v2.012A1.16 1.16 0 0 0 7.35 10.5h5.775a1.16 1.16 0 0 0 1.138-1.138V7.349a1.16 1.16 0 0 0-1.138-1.138H7.35zM22.662 16.713H5.337c-.525 0-.875-.35-.875-.875s.35-.875.875-.875h17.237c.525 0 .875.35.875.875s-.35.875-.787.875zM15.138 21.262h-9.8c-.525 0-.875-.35-.875-.875s.35-.875.875-.875h9.713c.525 0 .875.35.875.875s-.35.875-.787.875z'/></svg>",
                IconColor = "#3389ff",
                Display = "Content changed",
                Description = "For content changes like created, updated, published, unpublished..."
            },

            ["AssetChanged"] = new RuleElement
            {
                IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 28 28'><path d='M21.875 28H6.125A6.087 6.087 0 0 1 0 21.875V6.125A6.087 6.087 0 0 1 6.125 0h15.75A6.087 6.087 0 0 1 28 6.125v15.75A6.088 6.088 0 0 1 21.875 28zM6.125 1.75A4.333 4.333 0 0 0 1.75 6.125v15.75a4.333 4.333 0 0 0 4.375 4.375h15.75a4.333 4.333 0 0 0 4.375-4.375V6.125a4.333 4.333 0 0 0-4.375-4.375H6.125z'/><path d='M21.088 23.537H9.1c-.35 0-.612-.175-.787-.525s-.088-.7.088-.962l8.225-9.713c.175-.175.438-.35.7-.35s.525.175.7.35l5.25 7.525c.088.087.088.175.088.262.438 1.225.087 2.012-.175 2.45-.613.875-1.925.963-2.1.963zm-10.063-1.75h10.15c.175 0 .612-.088.7-.262.088-.088.088-.35 0-.7l-4.55-6.475-6.3 7.438zM9.1 13.737c-2.1 0-3.85-1.75-3.85-3.85S7 6.037 9.1 6.037s3.85 1.75 3.85 3.85-1.663 3.85-3.85 3.85zm0-5.949c-1.138 0-2.1.875-2.1 2.1s.962 2.1 2.1 2.1 2.1-.962 2.1-2.1-.875-2.1-2.1-2.1z'/></svg>",
                IconColor = "#3389ff",
                Display = "Asset changed",
                Description = "For asset changes like uploaded, updated, renamed, deleted..."
            }
        };

        public static IReadOnlyDictionary<string, RuleElement> Triggers
        {
            get { return TriggerTypes; }
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
                var name = actionType.Name;

                if (name.EndsWith(Suffix, StringComparison.Ordinal))
                {
                    name = name.Substring(0, name.Length - Suffix.Length);
                }

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

        public static void RegisterTypes(TypeNameRegistry typeNameRegistry)
        {
            foreach (var actionType in ActionTypes.Values)
            {
                typeNameRegistry.Map(actionType.Type, actionType.Type.Name);
            }
        }
    }
}
