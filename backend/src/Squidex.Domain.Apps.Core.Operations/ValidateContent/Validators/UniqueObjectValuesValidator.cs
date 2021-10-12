// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class UniqueObjectValuesValidator : IValidator
    {
        private readonly IEnumerable<string> fields;

        public UniqueObjectValuesValidator(IEnumerable<string> fields)
        {
            this.fields = fields;
        }

        public Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (value is IEnumerable<JsonObject> objects && objects.Count() > 1)
            {
                Validate(context, addError, objects);
            }
            else if (value is IEnumerable<Component> components && components.Count() > 1)
            {
                Validate(context, addError, components.Select(x => x.Data));
            }

            return Task.CompletedTask;
        }

        private void Validate(ValidationContext context, AddError addError, IEnumerable<JsonObject> items)
        {
            var duplicates = new HashSet<IJsonValue>(10);

            foreach (var field in fields)
            {
                duplicates.Clear();

                foreach (var item in items)
                {
                    if (item.TryGetValue(field, out var fieldValue) && !duplicates.Add(fieldValue))
                    {
                        addError(context.Path, T.Get("contents.validation.uniqueObjectValues", new { field }));
                        break;
                    }
                }
            }
        }
    }
}
