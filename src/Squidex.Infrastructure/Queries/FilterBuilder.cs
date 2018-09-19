// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using NodaTime;

namespace Squidex.Infrastructure.Queries
{
    public static class FilterBuilder
    {
        public static FilterJunction And(params FilterNode[] operands)
        {
            return new FilterJunction(FilterJunctionType.And, operands);
        }

        public static FilterJunction Or(params FilterNode[] operands)
        {
            return new FilterJunction(FilterJunctionType.Or, operands);
        }

        public static FilterComparison Eq(string path, string value)
        {
            return Binary(path, FilterOperator.Equals, value);
        }

        public static FilterComparison Eq(string path, bool value)
        {
            return Binary(path, FilterOperator.Equals, value);
        }

        public static FilterComparison Eq(string path, long value)
        {
            return Binary(path, FilterOperator.Equals, value);
        }

        public static FilterComparison Eq(string path, int value)
        {
            return Binary(path, FilterOperator.Equals, value);
        }

        public static FilterComparison Eq(string path, Instant value)
        {
            return Binary(path, FilterOperator.Equals, value);
        }

        public static FilterComparison In(string path, params long[] value)
        {
            return new FilterComparison(path.Split('.', '/'), FilterOperator.In, new FilterValue(value.ToList()));
        }

        private static FilterComparison Binary(string path, FilterOperator @operator, string value)
        {
            return new FilterComparison(path.Split('.', '/'), @operator, new FilterValue(value));
        }

        private static FilterComparison Binary(string path, FilterOperator @operator, bool value)
        {
            return new FilterComparison(path.Split('.', '/'), @operator, new FilterValue(value));
        }

        private static FilterComparison Binary(string path, FilterOperator @operator, long value)
        {
            return new FilterComparison(path.Split('.', '/'), @operator, new FilterValue(value));
        }

        private static FilterComparison Binary(string path, FilterOperator @operator, int value)
        {
            return new FilterComparison(path.Split('.', '/'), @operator, new FilterValue(value));
        }

        private static FilterComparison Binary(string path, FilterOperator @operator, Instant value)
        {
            return new FilterComparison(path.Split('.', '/'), @operator, new FilterValue(value));
        }
    }
}
