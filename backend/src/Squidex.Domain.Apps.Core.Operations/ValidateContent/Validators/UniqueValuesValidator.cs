// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
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

                foreach (var duplicate in itemsArray.Duplicates())
                {
                    addError(context.Path, $"{duplicate} has been added more than once.");
                }
            }

            return TaskHelper.Done;
        }
    }
}
