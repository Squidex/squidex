// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        public static FilterComparison Eq(string path, object value)
        {
            return Binary(path, FilterOperator.Equals, value);
        }

        private static FilterComparison Binary(string path, FilterOperator @operator, object value)
        {
            return new FilterComparison(path.Split('.', '/'), @operator, value, FilterValueType.String);
        }
    }
}
