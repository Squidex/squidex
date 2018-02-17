// ==========================================================================
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
                errors.Add(new ValidationError("Api key is required.", nameof(action.ApiKey)));
            }

            if (string.IsNullOrWhiteSpace(action.AppId))
            {
                errors.Add(new ValidationError("Application ID key is required.", nameof(action.AppId)));
            }

            if (string.IsNullOrWhiteSpace(action.IndexName))
            {
                errors.Add(new ValidationError("Index name is required.", nameof(action.IndexName)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }

        public Task<IEnumerable<ValidationError>> Visit(AzureQueueAction action)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(action.ConnectionString))
            {
                errors.Add(new ValidationError("Connection string is required.", nameof(action.ConnectionString)));
            }

            if (string.IsNullOrWhiteSpace(action.Queue))
            {
                errors.Add(new ValidationError("Queue is required.", nameof(action.Queue)));
            }
            else if (!Regex.IsMatch(action.Queue, "^[a-z][a-z0-9]{2,}(\\-[a-z0-9]+)*$"))
            {
                errors.Add(new ValidationError("Queue must be valid azure queue name.", nameof(action.Queue)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }

        public Task<IEnumerable<ValidationError>> Visit(ElasticSearchAction action)
        {
            var errors = new List<ValidationError>();

            if (action.Host == null || !action.Host.IsAbsoluteUri)
            {
                errors.Add(new ValidationError("Host is required and must be an absolute URL.", nameof(action.Host)));
            }

            if (string.IsNullOrWhiteSpace(action.IndexType))
            {
                errors.Add(new ValidationError("TypeName is required.", nameof(action.IndexType)));
            }

            if (string.IsNullOrWhiteSpace(action.IndexName))
            {
                errors.Add(new ValidationError("IndexName is required.", nameof(action.IndexName)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }

        public Task<IEnumerable<ValidationError>> Visit(FastlyAction action)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(action.ApiKey))
            {
                errors.Add(new ValidationError("Api key is required.", nameof(action.ApiKey)));
            }

            if (string.IsNullOrWhiteSpace(action.ServiceId))
            {
                errors.Add(new ValidationError("Service ID is required.", nameof(action.ServiceId)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }

        public Task<IEnumerable<ValidationError>> Visit(SlackAction action)
        {
            var errors = new List<ValidationError>();

            if (action.WebhookUrl == null || !action.WebhookUrl.IsAbsoluteUri)
            {
                errors.Add(new ValidationError("Webhook Url is required and must be an absolute URL.", nameof(action.WebhookUrl)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }

        public Task<IEnumerable<ValidationError>> Visit(WebhookAction action)
        {
            var errors = new List<ValidationError>();

            if (action.Url == null || !action.Url.IsAbsoluteUri)
            {
                errors.Add(new ValidationError("Url is required and must be an absolute URL.", nameof(action.Url)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }
    }
}
