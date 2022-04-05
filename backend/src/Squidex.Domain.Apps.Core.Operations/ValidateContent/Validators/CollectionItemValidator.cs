// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class CollectionItemValidator : IValidator
    {
        private readonly IValidator itemValidator;

        public CollectionItemValidator(IValidator itemValidator)
        {
            Guard.NotNull(itemValidator);

            this.itemValidator = itemValidator;
        }

        public async ValueTask ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (value is ICollection { Count: > 0 } items)
            {
                var targets = items.OfType<object>().Select((item, index) =>
                {
                    var innerContext = context.Nested($"[{index + 1}]");

                    return (item, innerContext);
                });

                await AsyncHelper.WhenAllThrottledAsync(targets, async (x, _) =>
                {
                    await itemValidator.ValidateAsync(x.item, x.innerContext, addError);
                });
            }
        }
    }
}
