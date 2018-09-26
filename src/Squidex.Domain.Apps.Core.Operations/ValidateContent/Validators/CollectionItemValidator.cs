// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
        private readonly IValidator[] itemValidators;

        public CollectionItemValidator(params IValidator[] itemValidators)
        {
            Guard.NotNull(itemValidators, nameof(itemValidators));
            Guard.NotEmpty(itemValidators, nameof(itemValidators));

            this.itemValidators = itemValidators;
        }

        public async Task ValidateAsync(object value, ValidationContext context, AddError addError)
        {
            if (value is ICollection items && items.Count > 0)
            {
                var innerTasks = new List<Task>();
                var index = 1;

                foreach (var item in items)
                {
                    var innerContext = context.Nested($"[{index}]");

                    foreach (var itemValidator in itemValidators)
                    {
                        innerTasks.Add(itemValidator.ValidateAsync(item, innerContext, addError));
                    }

                    index++;
                }

                await Task.WhenAll(innerTasks);
            }
        }
    }
}
