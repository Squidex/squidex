// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class UniqueValuesValidator<T> : IValidator
    {
        public Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (value is IEnumerable<T> items && items.Any())
            {
                var itemsArray = items.ToArray();

                if (itemsArray.Length != itemsArray.Distinct().Count())
                {
                    addError(context.Path, "Must not contain duplicate values.");
                }
            }

            return TaskHelper.Done;
        }
    }
}
