// ==========================================================================
//  RuleActionValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
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

        public Task<IEnumerable<ValidationError>> Visit(WebhookAction action)
        {
            var errors = new List<ValidationError>();

            if (action.Url == null || !action.Url.IsAbsoluteUri)
            {
                errors.Add(new ValidationError("Url must be specified and absolute.", nameof(action.Url)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }
    }
}
