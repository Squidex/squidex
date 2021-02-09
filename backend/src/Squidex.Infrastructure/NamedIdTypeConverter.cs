// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel;
using System.Globalization;

namespace Squidex.Infrastructure
{
    internal sealed class NamedIdTypeConverter : TypeConverter
    {
        private static readonly Parser<Guid> ParserGuid = Guid.TryParse;
        private static readonly Parser<DomainId> ParserDomainId = ParseDomainId;
        private static readonly Parser<string> ParserString = ParseString;
        private static readonly Parser<long> ParserLong = long.TryParse;
        private readonly Func<string, object>? converter;

        public NamedIdTypeConverter(Type type)
        {
            var genericType = type?.GetGenericArguments()?[0];

            if (genericType == typeof(Guid))
            {
                converter = v => NamedId<Guid>.Parse(v, ParserGuid);
            }
            else if (genericType == typeof(DomainId))
            {
                converter = v => NamedId<DomainId>.Parse(v, ParserDomainId);
            }
            else if (genericType == typeof(string))
            {
                converter = v => NamedId<string>.Parse(v, ParserString);
            }
            else if (genericType == typeof(long))
            {
                converter = v => NamedId<long>.Parse(v, ParserLong);
            }
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (converter == null)
            {
                throw new NotSupportedException();
            }

            return converter((string)value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return value.ToString()!;
        }

        private static bool ParseDomainId(ReadOnlySpan<char> value, out DomainId result)
        {
            result = DomainId.Create(new string(value));

            return true;
        }

        private static bool ParseString(ReadOnlySpan<char> value, out string result)
        {
            result = new string(value);

            return true;
        }
    }
}
