// ==========================================================================
//  WebhookActionValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Actions
{
    public class WebhookActionValidator : IRuleActionValidator
    {
        public IList<ValidationError> Validate(RuleAction ruleAction)
        {
            var errors = new List<ValidationError>();

            if (!(ruleAction is WebhookAction action))
                return errors;

            if (action.Url == null || !action.Url.IsAbsoluteUri)
            {
                errors.Add(new ValidationError("Url must be specified and absolute.", nameof(action.Url)));
            }

            return errors;
        }
    }
}