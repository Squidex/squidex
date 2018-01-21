// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        public Task<IEnumerable<ValidationError>> Visit(RuleAction action)
        {
            var errors = action.Validator.Validate(action); // todo: lazy way of doing this... needs better solution.

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }
    }
}
