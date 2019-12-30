﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NodaTime;

namespace Squidex.Infrastructure.Queries
{
    public sealed class ClrValue
    {
        public static readonly ClrValue Null = new ClrValue(null, ClrValueType.Null, false);

        public object? Value { get; }

        public ClrValueType ValueType { get; }

        public bool IsList { get; }

        private ClrValue(object? value, ClrValueType valueType, bool isList)
        {
            Value = value;
            ValueType = valueType;

            IsList = isList;
        }

        public static implicit operator ClrValue(Instant value)
        {
            return new ClrValue(value, ClrValueType.Instant, false);
        }

        public static implicit operator ClrValue(Guid value)
        {
            return new ClrValue(value, ClrValueType.Guid, false);
        }

        public static implicit operator ClrValue(bool value)
        {
            return new ClrValue(value, ClrValueType.Boolean, false);
        }

        public static implicit operator ClrValue(float value)
        {
            return new ClrValue(value, ClrValueType.Single, false);
        }

        public static implicit operator ClrValue(double value)
        {
            return new ClrValue(value, ClrValueType.Double, false);
        }

        public static implicit operator ClrValue(int value)
        {
            return new ClrValue(value, ClrValueType.Int32, false);
        }

        public static implicit operator ClrValue(long value)
        {
            return new ClrValue(value, ClrValueType.Int64, false);
        }

        public static implicit operator ClrValue(string? value)
        {
            return value != null ? new ClrValue(value, ClrValueType.String, false) : Null;
        }

        public static implicit operator ClrValue(List<Instant> value)
        {
            return value != null ? new ClrValue(value, ClrValueType.Instant, true) : Null;
        }

        public static implicit operator ClrValue(List<Guid> value)
        {
            return value != null ? new ClrValue(value, ClrValueType.Guid, true) : Null;
        }

        public static implicit operator ClrValue(List<bool> value)
        {
            return value != null ? new ClrValue(value, ClrValueType.Boolean, true) : Null;
        }

        public static implicit operator ClrValue(List<float> value)
        {
            return value != null ? new ClrValue(value, ClrValueType.Single, true) : Null;
        }

        public static implicit operator ClrValue(List<double> value)
        {
            return value != null ? new ClrValue(value, ClrValueType.Double, true) : Null;
        }

        public static implicit operator ClrValue(List<int> value)
        {
            return value != null ? new ClrValue(value, ClrValueType.Int32, true) : Null;
        }

        public static implicit operator ClrValue(List<long> value)
        {
            return value != null ? new ClrValue(value, ClrValueType.Int64, true) : Null;
        }

        public static implicit operator ClrValue(List<string> value)
        {
            return value != null ? new ClrValue(value, ClrValueType.String, true) : Null;
        }

        public static implicit operator ClrValue(List<object?> value)
        {
            return value != null ? new ClrValue(value, ClrValueType.Dynamic, true) : Null;
        }

        public override string ToString()
        {
            if (Value is IList list)
            {
                return $"[{string.Join(", ", list.OfType<object>().Select(ToString).ToArray())}]";
            }

            return ToString(Value);
        }

        private static string ToString(object? value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is string s)
            {
                return $"'{s.Replace("'", "\\'")}'";
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }
    }
}
