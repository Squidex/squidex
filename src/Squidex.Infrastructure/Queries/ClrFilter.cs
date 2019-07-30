﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries
{
    public static class ClrFilter
    {
        public static LogicalFilter<ClrValue> And(params FilterNode<ClrValue>[] filters)
        {
            return new LogicalFilter<ClrValue>(LogicalFilterType.And, filters);
        }

        public static LogicalFilter<ClrValue> Or(params FilterNode<ClrValue>[] filters)
        {
            return new LogicalFilter<ClrValue>(LogicalFilterType.Or, filters);
        }

        public static NegateFilter<ClrValue> Not(FilterNode<ClrValue> filter)
        {
            return new NegateFilter<ClrValue>(filter);
        }

        public static CompareFilter<ClrValue> Eq(PropertyPath path, ClrValue value)
        {
            return Binary(path, CompareOperator.Equals, value);
        }

        public static CompareFilter<ClrValue> Ne(PropertyPath path, ClrValue value)
        {
            return Binary(path, CompareOperator.NotEquals, value);
        }

        public static CompareFilter<ClrValue> Lt(PropertyPath path, ClrValue value)
        {
            return Binary(path, CompareOperator.LessThan, value);
        }

        public static CompareFilter<ClrValue> Le(PropertyPath path, ClrValue value)
        {
            return Binary(path, CompareOperator.LessThanOrEqual, value);
        }

        public static CompareFilter<ClrValue> Gt(PropertyPath path, ClrValue value)
        {
            return Binary(path, CompareOperator.GreaterThan, value);
        }

        public static CompareFilter<ClrValue> Ge(PropertyPath path, ClrValue value)
        {
            return Binary(path, CompareOperator.GreaterThanOrEqual, value);
        }

        public static CompareFilter<ClrValue> Contains(PropertyPath path, ClrValue value)
        {
            return Binary(path, CompareOperator.Contains, value);
        }

        public static CompareFilter<ClrValue> EndsWith(PropertyPath path, ClrValue value)
        {
            return Binary(path, CompareOperator.EndsWith, value);
        }

        public static CompareFilter<ClrValue> StartsWith(PropertyPath path, ClrValue value)
        {
            return Binary(path, CompareOperator.StartsWith, value);
        }

        public static CompareFilter<ClrValue> Empty(PropertyPath path)
        {
            return Binary(path, CompareOperator.Empty, null);
        }

        public static CompareFilter<ClrValue> In(PropertyPath path, ClrValue value)
        {
            return Binary(path, CompareOperator.In, value);
        }

        private static CompareFilter<ClrValue> Binary(PropertyPath path, CompareOperator @operator, ClrValue value)
        {
            return new CompareFilter<ClrValue>(path, @operator, value ?? ClrValue.Null);
        }
    }
}
