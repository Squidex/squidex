// ==========================================================================
//  ElasticSearchActionValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Actions
{
    public class ElasticSearchActionValidator : IRuleActionValidator
    {
        public IList<ValidationError> Validate(RuleAction ruleAction)
        {
            var result = new List<ValidationError>();

            if (!(ruleAction is ElasticSearchAction action))
            {
                result.Add(new ValidationError("Rule action type mismatch"));
                return result;
            }

            if (action.RequiresAuthentication && string.IsNullOrWhiteSpace(action.Username))
            {
                result.Add(new ValidationError("Username must be defined.", nameof(action.Username)));
            }

            if (action.RequiresAuthentication && string.IsNullOrWhiteSpace(action.Password))
            {
                result.Add(new ValidationError("Password must be defined.", nameof(action.Password)));
            }

            if (string.IsNullOrWhiteSpace(action.HostUrl))
            {
                result.Add(new ValidationError("HostUrl must be defined.", nameof(action.HostUrl)));
            }

            if (string.IsNullOrWhiteSpace(action.TypeNameForSchema))
            {
                result.Add(new ValidationError("Type name must be defined.", nameof(action.TypeNameForSchema)));
            }

            if (string.IsNullOrWhiteSpace(action.IndexName))
            {
                result.Add(new ValidationError("Index name must be defined.", nameof(action.IndexName)));
            }

            if (!string.IsNullOrWhiteSpace(action.HostUrl) &&
                !Uri.TryCreate(action.HostUrl, UriKind.Absolute, out var dummy))
            {
                result.Add(new ValidationError("Invalid host url.", nameof(action.IndexName)));
            }

            return result;
        }
    }
}