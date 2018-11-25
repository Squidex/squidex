// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using NodaTime;
using NodaTime.Text;

namespace Squidex.Infrastructure
{
    public sealed class PropertyValue : DynamicObject
    {
        private readonly object rawValue;

        private static readonly Dictionary<Type, Func<PropertyValue, object>> Parsers =
            new Dictionary<Type, Func<PropertyValue, object>>
            {
                { typeof(string),   p => p.ToString() },
                { typeof(bool),     p => p.ToBoolean() },
                { typeof(bool?),    p => p.ToNullableBoolean() },
                { typeof(float),    p => p.ToSingle() },
                { typeof(float?),   p => p.ToNullableSingle() },
                { typeof(double),   p => p.ToDouble() },
                { typeof(double?),  p => p.ToNullableDouble() },
                { typeof(int),      p => p.ToInt32() },
                { typeof(int?),     p => p.ToNullableInt32() },
                { typeof(long),     p => p.ToInt64() },
                { typeof(long?),    p => p.ToNullableInt64() },
                { typeof(Instant),  p => p.ToInstant() },
                { typeof(Instant?), p => p.ToNullableInstant() },
                { typeof(Guid),     p => p.ToGuid() },
                { typeof(Guid?),    p => p.ToNullableGuid() }
            };

        public object RawValue
        {
            get { return rawValue; }
        }

        internal PropertyValue(object rawValue)
        {
            if (rawValue != null && !Parsers.ContainsKey(rawValue.GetType()))
            {
                throw new InvalidOperationException($"The type '{rawValue.GetType()}' is not supported.");
            }

            this.rawValue = rawValue;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;

            if (!Parsers.TryGetValue(binder.Type, out var parser))
            {
                return false;
            }

            result = parser(this);

            return true;
        }

        public override string ToString()
        {
            return rawValue?.ToString();
        }

        public bool ToBoolean()
        {
            return ToOrParseValue(CultureInfo.InvariantCulture, ParseBoolean);
        }

        public bool? ToNullableBoolean()
        {
            return ToNullableOrParseValue(CultureInfo.InvariantCulture, ParseBoolean);
        }

        public float ToSingle()
        {
            return ToOrParseValue(CultureInfo.InvariantCulture, x => float.Parse(x, CultureInfo.InvariantCulture));
        }

        public float? ToNullableSingle()
        {
            return ToNullableOrParseValue(CultureInfo.InvariantCulture, x => float.Parse(x, CultureInfo.InvariantCulture));
        }

        public double ToDouble()
        {
            return ToOrParseValue(CultureInfo.InvariantCulture, x => double.Parse(x, CultureInfo.InvariantCulture));
        }

        public double? ToNullableDouble()
        {
            return ToNullableOrParseValue(CultureInfo.InvariantCulture, x => double.Parse(x, CultureInfo.InvariantCulture));
        }

        public int ToInt32()
        {
            return ToOrParseValue(CultureInfo.InvariantCulture, x => int.Parse(x, CultureInfo.InvariantCulture));
        }

        public int? ToNullableInt32()
        {
            return ToNullableOrParseValue(CultureInfo.InvariantCulture, x => int.Parse(x, CultureInfo.InvariantCulture));
        }

        public long ToInt64()
        {
            return ToOrParseValue(CultureInfo.InvariantCulture, x => long.Parse(x, CultureInfo.InvariantCulture));
        }

        public long? ToNullableInt64()
        {
            return ToNullableOrParseValue(CultureInfo.InvariantCulture, x => long.Parse(x, CultureInfo.InvariantCulture));
        }

        public Instant ToInstant()
        {
            return ToOrParseValue(CultureInfo.InvariantCulture, x => InstantPattern.General.Parse(x).Value);
        }

        public Instant? ToNullableInstant()
        {
            return ToNullableOrParseValue(CultureInfo.InvariantCulture, x => InstantPattern.General.Parse(x).Value);
        }

        public Guid ToGuid()
        {
            return ToOrParseValue(CultureInfo.InvariantCulture, Guid.Parse);
        }

        public Guid? ToNullableGuid()
        {
            return ToNullableOrParseValue(CultureInfo.InvariantCulture, Guid.Parse);
        }

        private T? ToNullableOrParseValue<T>(IFormatProvider culture, Func<string, T> parser) where T : struct
        {
            return TryParse(culture, parser, out var result) ? result : (T?)null;
        }

        private T ToOrParseValue<T>(IFormatProvider culture, Func<string, T> parser)
        {
            return TryParse(culture, parser, out var result) ? result : default(T);
        }

        private bool TryParse<T>(IFormatProvider culture, Func<string, T> parser, out T result)
        {
            var value = rawValue;

            if (value != null)
            {
                var valueType = value.GetType();

                if (valueType == typeof(T))
                {
                    result = (T)value;
                }
                else if (valueType == typeof(string))
                {
                    result = Parse(parser, valueType, value);
                }
                else
                {
                    result = Convert<T>(culture, value, valueType);
                }

                return true;
            }

            result = default(T);

            return false;
        }

        private static T Convert<T>(IFormatProvider culture, object value, Type valueType)
        {
            var requestedType = typeof(T);

            try
            {
                return (T)System.Convert.ChangeType(value, requestedType, culture);
            }
            catch (OverflowException)
            {
                var message = $"The property has type '{valueType}' and cannot be casted to '{requestedType}' because it is either too small or large.";

                throw new InvalidCastException(message);
            }
            catch (InvalidCastException)
            {
                var message = $"The property has type '{valueType}' and cannot be casted to '{requestedType}'.";

                throw new InvalidCastException(message);
            }
        }

        private static T Parse<T>(Func<string, T> parser, Type valueType, object value)
        {
            var requestedType = typeof(T);

            try
            {
                return parser(value.ToString());
            }
            catch (Exception ex)
            {
                var message = $"The property has type '{valueType}' and cannot be casted to '{requestedType}'.";

                throw new InvalidCastException(message, ex);
            }
        }

        private static bool ParseBoolean(string value)
        {
            switch (value)
            {
                case "1":
                    return true;
                case "0":
                    return false;
                default:
                    return bool.Parse(value);
            }
        }
    }
}
