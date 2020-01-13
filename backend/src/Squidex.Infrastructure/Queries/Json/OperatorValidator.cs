﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using NJsonSchema;

namespace Squidex.Infrastructure.Queries.Json
{
    public static class OperatorValidator
    {
        private static readonly CompareOperator[] BooleanOperators =
        {
            CompareOperator.Equals,
            CompareOperator.In,
            CompareOperator.NotEquals
        };
        private static readonly CompareOperator[] NumberOperators =
        {
            CompareOperator.Equals,
            CompareOperator.LessThan,
            CompareOperator.LessThanOrEqual,
            CompareOperator.GreaterThan,
            CompareOperator.GreaterThanOrEqual,
            CompareOperator.In,
            CompareOperator.NotEquals
        };
        private static readonly CompareOperator[] StringOperators =
        {
            CompareOperator.Contains,
            CompareOperator.Empty,
            CompareOperator.EndsWith,
            CompareOperator.Equals,
            CompareOperator.GreaterThan,
            CompareOperator.GreaterThanOrEqual,
            CompareOperator.In,
            CompareOperator.LessThan,
            CompareOperator.LessThanOrEqual,
            CompareOperator.NotEquals,
            CompareOperator.StartsWith
        };
        private static readonly CompareOperator[] ArrayOperators =
        {
            CompareOperator.Empty,
            CompareOperator.Equals,
            CompareOperator.In,
            CompareOperator.NotEquals
        };

        public static bool IsAllowedOperator(JsonSchema schema, CompareOperator compareOperator)
        {
            switch (schema.Type)
            {
                case JsonObjectType.None:
                    return true;
                case JsonObjectType.Boolean:
                    return BooleanOperators.Contains(compareOperator);
                case JsonObjectType.Integer:
                case JsonObjectType.Number:
                    return NumberOperators.Contains(compareOperator);
                case JsonObjectType.String:
                    return StringOperators.Contains(compareOperator);
                case JsonObjectType.Array:
                    return ArrayOperators.Contains(compareOperator);
            }

            return false;
        }
    }
}
