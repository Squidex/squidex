// ==========================================================================
//  PropertyValue.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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

        private static readonly Dictionary<Type, Func<PropertyValue, CultureInfo, object>> Parsers =
            new Dictionary<Type, Func<PropertyValue, CultureInfo, object>>
            {
                { typeof(string),   (p, c) => p.ToString() },
                { typeof(bool),     (p, c) => p.ToBoolean(c) },
                { typeof(bool?),    (p, c) => p.ToNullableBoolean(c) },
                { typeof(float),    (p, c) => p.ToSingle(c) },
                { typeof(float?),   (p, c) => p.ToNullableSingle(c) },
                { typeof(double),   (p, c) => p.ToDouble(c) },
                { typeof(double?),  (p, c) => p.ToNullableDouble(c) },
                { typeof(int),      (p, c) => p.ToInt32(c) },
                { typeof(int?),     (p, c) => p.ToNullableInt32(c) },
                { typeof(long),     (p, c) => p.ToInt64(c) },
                { typeof(long?),    (p, c) => p.ToNullableInt64(c) },
                { typeof(Instant),  (p, c) => p.ToInstant(c) },
                { typeof(Instant?), (p, c) => p.ToNullableInstant(c) },
                { typeof(Guid),     (p, c) => p.ToGuid(c) },
                { typeof(Guid?),    (p, c) => p.ToNullableGuid(c) }
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

            result = parser(this, CultureInfo.InvariantCulture);

            return true;
        }

        public override string ToString()
        {
            return rawValue?.ToString();
        }

        public bool ToBoolean(CultureInfo culture)
        {
            return ToOrParseValue(culture, ParseBoolean);
        }

        public bool? ToNullableBoolean(CultureInfo culture)
        {
            return ToNullableOrParseValue(culture, ParseBoolean);
        }

        public float ToSingle(CultureInfo culture)
        {
            return ToOrParseValue(culture, x => float.Parse(x, culture));
        }

        public float? ToNullableSingle(CultureInfo culture)
        {
            return ToNullableOrParseValue(culture, x => float.Parse(x, culture));
        }

        public double ToDouble(CultureInfo culture)
        {
            return ToOrParseValue(culture, x => double.Parse(x, culture));
        }

        public double? ToNullableDouble(CultureInfo culture)
        {
            return ToNullableOrParseValue(culture, x => double.Parse(x, culture));
        }

        public int ToInt32(CultureInfo culture)
        {
            return ToOrParseValue(culture, x => int.Parse(x, culture));
        }

        public int? ToNullableInt32(CultureInfo culture)
        {
            return ToNullableOrParseValue(culture, x => int.Parse(x, culture));
        }

        public long ToInt64(CultureInfo culture)
        {
            return ToOrParseValue(culture, x => long.Parse(x, culture));
        }

        public long? ToNullableInt64(CultureInfo culture)
        {
            return ToNullableOrParseValue(culture, x => long.Parse(x, culture));
        }

        public Instant ToInstant(CultureInfo culture)
        {
            return ToOrParseValue(culture, x => InstantPattern.General.Parse(x).Value);
        }

        public Instant? ToNullableInstant(CultureInfo culture)
        {
            return ToNullableOrParseValue(culture, x => InstantPattern.General.Parse(x).Value);
        }

        public Guid ToGuid(CultureInfo culture)
        {
            return ToOrParseValue(culture, Guid.Parse);
        }

        public Guid? ToNullableGuid(CultureInfo culture)
        {
            return ToNullableOrParseValue(culture, Guid.Parse);
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
