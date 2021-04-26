// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class CollectionItemValidator : IValidator
    {
        private readonly IValidator itemValidator;

        public CollectionItemValidator(IValidator itemValidator)
        {
            Guard.NotNull(itemValidator, nameof(itemValidator));

            this.itemValidator = itemValidator;
        }

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (value is ICollection { Count: > 0 } items)
            {
                var innerTasks = new List<Task>();
                var index = 1;

                foreach (var item in items)
                {
                    var innerContext = context.Nested($"[{index}]");

                    await itemValidator.ValidateAsync(item, innerContext, addError);

                    index++;
                }

                await Task.WhenAll(innerTasks);
            }
        }
    }
}
