// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Fluid;
using Fluid.Values;
using NodaTime;
using NodaTime.Text;

namespace Squidex.Domain.Apps.Core.Templates.Extensions;

public class DateTimeFluidExtension : IFluidExtension
{
    private static readonly FilterDelegate FormatDate = (input, arguments, context) =>
    {
        if (arguments.Count == 1)
        {
            return FormatDateCore(input, x => Format(arguments, x));
        }

        return input;
    };

    private static readonly FilterDelegate FormatTimestamp = (input, arguments, context) =>
    {
        return FormatDateCore(input, x => NumberValue.Create(x.ToUnixTimeMilliseconds()));
    };

    private static readonly FilterDelegate FormatTimestampSec = (input, arguments, context) =>
    {
        return FormatDateCore(input, x => NumberValue.Create(x.ToUnixTimeMilliseconds() / 1000));
    };

    public void RegisterLanguageExtensions(CustomFluidParser parser, TemplateOptions options)
    {
        options.Filters.AddFilter("format_date", FormatDate);
        options.Filters.AddFilter("timestamp", FormatTimestamp);
        options.Filters.AddFilter("timestamp_sec", FormatTimestampSec);
    }

    private static ValueTask<FluidValue> FormatDateCore(FluidValue input, Func<DateTimeOffset, FluidValue> formatter)
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
