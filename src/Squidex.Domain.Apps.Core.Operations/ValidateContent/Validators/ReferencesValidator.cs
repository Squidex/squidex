// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class ReferencesValidator : IValidator
    {
        private static readonly IReadOnlyList<string> Path = new List<string> { "Id" };

        private readonly Guid schemaId;

        public ReferencesValidator(Guid schemaId)
        {
            this.schemaId = schemaId;
        }

        public async Task ValidateAsync(object value, ValidationContext context, AddError addError)
        {
            if (value is ICollection<Guid> contentIds)
            {
                var filter = new FilterComparison(Path, FilterOperator.In, new FilterValue(contentIds.ToList()));

                var foundIds = await context.GetContentIdsAsync(schemaId, filter);

                foreach (var id in contentIds)
                {
                    if (!foundIds.Contains(id))
                    {
                        addError(context.Path, $"Contains invalid reference '{id}'.");
                    }
                }
            }
        }
    }
}
