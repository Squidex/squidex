// ==========================================================================
//  CollectionItemValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public sealed class CollectionItemValidator<T> : IValidator
    {
        private readonly IValidator[] itemValidators;

        public CollectionItemValidator(params IValidator[] itemValidators)
        {
            Guard.NotNull(itemValidators, nameof(itemValidators));
            Guard.NotEmpty(itemValidators, nameof(itemValidators));

            this.itemValidators = itemValidators;
        }

        public async Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (value is ICollection<T> items)
            {
                var innerContext = context.Optional(false);

                var index = 1;

                foreach (var item in items)
                {
                    foreach (var itemValidator in itemValidators)
                    {
                        await itemValidator.ValidateAsync(item, innerContext, e => addError(e.Replace("<FIELD>", $"<FIELD> item #{index}")));
                    }

                    index++;
                }
            }
        }
    }
}
