﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Guards
{
    public sealed class RuleActionValidator : IRuleActionVisitor<Task<IEnumerable<ValidationError>>>
    {
        public static Task<IEnumerable<ValidationError>> ValidateAsync(RuleAction action)
        {
            Guard.NotNull(action, nameof(action));

            var visitor = new RuleActionValidator();

            return action.Accept(visitor);
        }

        public Task<IEnumerable<ValidationError>> Visit(AlgoliaAction action)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(action.ApiKey))
            {
                errors.Add(new ValidationError("Api key must be defined.", nameof(action.ApiKey)));
            }

            if (string.IsNullOrWhiteSpace(action.AppId))
            {
                errors.Add(new ValidationError("Application ID key must be defined.", nameof(action.AppId)));
            }

            if (string.IsNullOrWhiteSpace(action.IndexName))
            {
                errors.Add(new ValidationError("Index name must be defined.", nameof(action.ApiKey)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }

        public Task<IEnumerable<ValidationError>> Visit(AzureQueueAction action)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(action.ConnectionString))
            {
                errors.Add(new ValidationError("Connection string must be defined.", nameof(action.ConnectionString)));
            }

            if (string.IsNullOrWhiteSpace(action.Queue))
            {
                errors.Add(new ValidationError("Queue must be defined.", nameof(action.Queue)));
            }
            else if (!Regex.IsMatch(action.Queue, "^[a-z][a-z0-9]{2,}(\\-[a-z0-9]+)*$"))
            {
                errors.Add(new ValidationError("Queue must be valid azure queue name.", nameof(action.Queue)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }

        public Task<IEnumerable<ValidationError>> Visit(FastlyAction action)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(action.ApiKey))
            {
                errors.Add(new ValidationError("Api key must be defined.", nameof(action.ApiKey)));
            }

            if (string.IsNullOrWhiteSpace(action.ServiceId))
            {
                errors.Add(new ValidationError("Service name must be defined.", nameof(action.ServiceId)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }

        public Task<IEnumerable<ValidationError>> Visit(SlackAction action)
        {
            var errors = new List<ValidationError>();

            if (action.WebhookUrl == null || !action.WebhookUrl.IsAbsoluteUri)
            {
                errors.Add(new ValidationError("Webhook Url must be specified and absolute.", nameof(action.WebhookUrl)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }

        public Task<IEnumerable<ValidationError>> Visit(WebhookAction action)
        {
            var errors = action.Validator.Validate(action); // todo: lazy way of doing this... needs better solution.

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }
    }
}
