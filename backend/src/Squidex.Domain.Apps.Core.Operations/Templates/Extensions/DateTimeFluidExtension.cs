// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using Fluid;
using Fluid.Values;
using NodaTime;
using NodaTime.Text;

namespace Squidex.Domain.Apps.Core.Templates.Extensions
{
    public class DateTimeFluidExtension : IFluidExtension
    {
        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            TemplateContext.GlobalFilters.AddFilter("format_date", FormatDate);

            TemplateContext.GlobalFilters.AddFilter("timestamp", FormatTimestamp);
            TemplateContext.GlobalFilters.AddFilter("timestamp_sec", FormatTimestampSec);
        }

        public static FluidValue FormatTimestamp(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return FormatDate(input, x => FluidValue.Create(x.ToUnixTimeMilliseconds()));
        }

        public static FluidValue FormatTimestampSec(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return FormatDate(input, x => FluidValue.Create(x.ToUnixTimeMilliseconds() / 1000));
        }

        public static FluidValue FormatDate(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (arguments.Count == 1)
            {
                return FormatDate(input, x => Format(arguments, x));
            }

            return input;
        }

        private static FluidValue FormatDate(FluidValue input, Func<DateTimeOffset, FluidValue> formatter)
        {
            switch (input)
            {
                case DateTimeValue dateTime:
                    {
                        var value = (DateTimeOffset)dateTime.ToObjectValue();

                        return formatter(value);
                    }

                case StringValue stringValue:
                    {
                        var value = stringValue.ToStringValue();

                        var instant = InstantPattern.ExtendedIso.Parse(value);

                        if (instant.Success)
                        {
                            return formatter(instant.Value.ToDateTimeOffset());
                        }

                        break;
                    }

                case ObjectValue objectValue:
                    {
                        var value = objectValue.ToObjectValue();

                        if (value is Instant instant)
                        {
                            return formatter(instant.ToDateTimeOffset());
                        }

                        break;
                    }
            }

            return input;
        }

        private static FluidValue Format(FilterArguments arguments, DateTimeOffset value)
        {
            var formatted = value.ToString(arguments.At(0).ToStringValue(), CultureInfo.InvariantCulture);

            return new StringValue(formatted);
        }
    }
}
