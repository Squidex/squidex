// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace Squidex.Infrastructure.Queries
{
    public sealed class FilterValue
    {
        public static readonly FilterValue Null = new FilterValue(null, FilterValueType.Null, false);

        public object Value { get; }

        public FilterValueType ValueType { get; }
        public bool IsList { get; }

        public FilterValue(Guid value)
            : this(value, FilterValueType.Guid, false)
        {
        }

        public FilterValue(Instant value)
            : this(value, FilterValueType.Instant, false)
        {
        }

        public FilterValue(bool value)
            : this(value, FilterValueType.Boolean, false)
        {
        }

        public FilterValue(float value)
            : this(value, FilterValueType.Single, false)
        {
        }

        public FilterValue(double value)
            : this(value, FilterValueType.Double, false)
        {
        }

        public FilterValue(int value)
            : this(value, FilterValueType.Int32, false)
        {
        }

        public FilterValue(long value)
            : this(value, FilterValueType.Int64, false)
        {
        }

        public FilterValue(string value)
            : this(value, FilterValueType.String, false)
        {
            Guard.NotNull(value, nameof(value));
        }

        public FilterValue(List<Guid> value)
            : this(value, FilterValueType.Guid, true)
        {
            Guard.NotNull(value, nameof(value));
        }

        public FilterValue(List<Instant> value)
            : this(value, FilterValueType.Instant, true)
        {
            Guard.NotNull(value, nameof(value));
        }

        public FilterValue(List<bool> value)
            : this(value, FilterValueType.Boolean, true)
        {
            Guard.NotNull(value, nameof(value));
        }

        public FilterValue(List<float> value)
            : this(value, FilterValueType.Single, true)
        {
            Guard.NotNull(value, nameof(value));
        }

        public FilterValue(List<double> value)
            : this(value, FilterValueType.Double, true)
        {
            Guard.NotNull(value, nameof(value));
        }

        public FilterValue(List<int> value)
            : this(value, FilterValueType.Int32, true)
        {
            Guard.NotNull(value, nameof(value));
        }

        public FilterValue(List<long> value)
            : this(value, FilterValueType.Int64, true)
        {
            Guard.NotNull(value, nameof(value));
        }

        public FilterValue(List<string> value)
            : this(value, FilterValueType.String, true)
        {
            Guard.NotNull(value, nameof(value));
        }

        private FilterValue(object value, FilterValueType valueType, bool isList)
        {
            Value = value;
            ValueType = valueType;

            IsList = isList;
        }

        public override string ToString()
        {
            if (Value is IList list)
            {
                return $"[{string.Join(", ", list.OfType<object>().Select(ToString).ToArray())}]";
            }
            else
            {
                return ToString(Value);
            }
        }

        private string ToString(object value)
        {
            if (ValueType == FilterValueType.String)
            {
                return $"'{value.ToString().Replace("'", "\\'")}'";
            }
            else if (value == null)
            {
                return "null";
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
