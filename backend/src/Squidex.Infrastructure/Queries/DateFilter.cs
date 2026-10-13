// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using NodaTime.Text;

namespace Squidex.Infrastructure.Queries;

public static class DateFilter
{
    private static readonly Duration OneDay = Duration.FromDays(1);

    public static bool IsDate(string? value)
    {
        return value != null && LocalDatePattern.Iso.Parse(value).Success;
    }

    public static FilterNode<ClrValue> Create(PropertyPath path, CompareOperator @operator, Instant date)
    {
        // A date covers the whole day, therefore we compare against the range [date, date + 1 day).
        var next = date.Plus(OneDay);

        switch (@operator)
        {
            case CompareOperator.Equals:
                return ClrFilter.And(ClrFilter.Ge(path, date), ClrFilter.Lt(path, next));
            case CompareOperator.NotEquals:
                return ClrFilter.Not(ClrFilter.And(ClrFilter.Ge(path, date), ClrFilter.Lt(path, next)));
            case CompareOperator.LessThan:
                return ClrFilter.Lt(path, date);
            case CompareOperator.LessThanOrEqual:
                return ClrFilter.Lt(path, next);
            case CompareOperator.GreaterThan:
                return ClrFilter.Ge(path, next);
            case CompareOperator.GreaterThanOrEqual:
                return ClrFilter.Ge(path, date);
            default:
                return new CompareFilter<ClrValue>(path, @operator, date);
        }
    }

    public static FilterNode<ClrValue> CreateIn(PropertyPath path, List<Instant> dates, List<Instant> instants)
    {
        var filters = dates.Select(x => Create(path, CompareOperator.Equals, x)).ToList();

        if (instants.Count > 0)
        {
            filters.Add(ClrFilter.In(path, instants));
        }

        if (filters.Count == 1)
        {
            return filters[0];
        }

        return ClrFilter.Or(filters);
    }
}
