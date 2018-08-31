// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Rules.Action.Algolia;
using Squidex.Domain.Apps.Rules.Action.AzureQueue;
using Squidex.Domain.Apps.Rules.Action.ElasticSearch;
using Squidex.Domain.Apps.Rules.Action.Fastly;
using Squidex.Domain.Apps.Rules.Action.Medium;
using Squidex.Domain.Apps.Rules.Action.Slack;
using Squidex.Domain.Apps.Rules.Action.Twitter;
using Squidex.Domain.Apps.Rules.Action.Webhook;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Rules.Actions
{
    public static class RuleActionRegistry
    {
        private const string Suffix = "Action";
        private static readonly HashSet<Type> ActionHandlerTypes = new HashSet<Type>();
        private static readonly Dictionary<string, Type> ActionTypes = new Dictionary<string, Type>();

        public static IReadOnlyDictionary<string, Type> Actions
        {
            get { return ActionTypes; }
        }

        public static IReadOnlyCollection<Type> ActionHandlers
        {
            get { return ActionHandlerTypes; }
        }

        static RuleActionRegistry()
        {
            Register<
                AlgoliaAction,
                AlgoliaActionHandler>();

            Register<
                AzureQueueAction,
                AzureQueueActionHandler>();

            Register<
                ElasticSearchAction,
                ElasticSearchActionHandler>();

            Register<
                FastlyAction,
                FastlyActionHandler>();

            Register<
                MediumAction,
                MediumActionHandler>();

            Register<
                SlackAction,
                SlackActionHandler>();

            Register<
                TweetAction,
                TweetActionHandler>();

            Register<
                WebhookAction,
                WebhookActionHandler>();
        }

        public static void Register<TAction, THandler>() where TAction : RuleAction where THandler : IRuleActionHandler
        {
            AddActionType<TAction>();
            AddActionHandler<THandler>();
        }

        private static void AddActionHandler<THandler>() where THandler : IRuleActionHandler
        {
            ActionHandlerTypes.Add(typeof(THandler));
        }

        private static void AddActionType<TAction>() where TAction : RuleAction
        {
            var name = typeof(TAction).Name;

            if (name.EndsWith(Suffix, StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - Suffix.Length);
            }

            ActionTypes.Add(name, typeof(TAction));
        }

        public static void RegisterTypes(TypeNameRegistry typeNameRegistry)
        {
            foreach (var actionType in ActionTypes.Values)
            {
                typeNameRegistry.Map(actionType, actionType.Name);
            }
        }
    }
}
