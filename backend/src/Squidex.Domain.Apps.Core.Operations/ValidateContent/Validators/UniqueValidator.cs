// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Translations;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public delegate Task<IReadOnlyList<ContentIdStatus>> CheckUniqueness(FilterNode<ClrValue> filter);

public sealed class UniqueValidator : IValidator
{
    private readonly CheckUniqueness checkUniqueness;

    public UniqueValidator(CheckUniqueness checkUniqueness)
    {
        this.checkUniqueness = checkUniqueness;
    }

    public void Validate(object? value, ValidationContext context)
    {
        var count = context.Path.Count();

        if (value != null && (count == 0 || (count == 2 && context.Path.Last() == InvariantPartitioning.Key)))
        {
            FilterNode<ClrValue>? filter = null;

            if (value is string s)
            {
                filter = ClrFilter.Eq(Path(context), s);
            }
            else if (value is double d)
            {
                filter = ClrFilter.Eq(Path(context), d);
            }

            if (filter != null)
            {
                context.Root.AddTask(ct => ValidateCoreAsync(context, filter));
            }
        }
    }

    private async Task ValidateCoreAsync(ValidationContext context, FilterNode<ClrValue> filter)
    {
        var found = await checkUniqueness(filter);

        if (found.Any(x => x.Id != context.Root.ContentId))
        {
            context.AddError(context.Path, T.Get("contents.validation.unique"));
        }
    }

    private static List<string> Path(ValidationContext context)
    {
        return Enumerable.Repeat("Data", 1).Union(context.Path).ToList();
    }
}
