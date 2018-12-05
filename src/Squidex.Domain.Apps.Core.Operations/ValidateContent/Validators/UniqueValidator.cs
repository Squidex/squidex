// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class UniqueValidator : IValidator
    {
        public async Task ValidateAsync(object value, ValidationContext context, AddError addError)
        {
            var count = context.Path.Count();

            if (value != null && (count == 0 || (count == 2 && context.Path.Last() == InvariantPartitioning.Instance.Master.Key)))
            {
                FilterNode filter = null;

                if (value is string s)
                {
                    filter = new FilterComparison(Path(context), FilterOperator.Equals, new FilterValue(s));
                }
                else if (value is double d)
                {
                    filter = new FilterComparison(Path(context), FilterOperator.Equals, new FilterValue(d));
                }

                if (filter != null)
                {
                    var found = await context.GetContentIdsAsync(context.SchemaId, filter);

                    if (found.Any(x => x != context.ContentId))
                    {
                        addError(context.Path, "Another content with the same value exists.");
                    }
                }
            }
        }

        private static List<string> Path(ValidationContext context)
        {
            return Enumerable.Repeat("Data", 1).Union(context.Path).ToList();
        }
    }
}
