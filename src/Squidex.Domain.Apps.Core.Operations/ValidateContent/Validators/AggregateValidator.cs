// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class AggregateValidator : IValidator
    {
        private readonly IValidator[] validators;

        public AggregateValidator(IEnumerable<IValidator> validators)
        {
            this.validators = validators?.ToArray();
        }

        public Task ValidateAsync(object value, ValidationContext context, AddError addError)
        {
            if (validators?.Length > 0)
            {
                return Task.WhenAll(validators.Select(x => x.ValidateAsync(value, context, addError)));
            }

            return Task.CompletedTask;
        }
    }
}
