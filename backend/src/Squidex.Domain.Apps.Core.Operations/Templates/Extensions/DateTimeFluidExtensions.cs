// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Fluid;
using Fluid.Values;
using NodaTime;

namespace Squidex.Domain.Apps.Core.Templates.Extensions
{
    public class DateTimeFluidExtensions : IFluidExtension
    {
        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            TemplateContext.GlobalFilters.AddFilter("formatDate", FormatDate);
        }

        public static FluidValue FormatDate(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (arguments.Count == 1)
            {
                switch (input)
                {
                    case DateTimeValue dateTime:
                        {
                            var value = (DateTimeOffset)dateTime.ToObjectValue();

                            return Format(arguments, value);
                        }

                    case ObjectValue objectValue:
                        {
                            var value = objectValue.ToObjectValue();

                            if (value is Instant instant)
                            {
                                return Format(arguments, instant.ToDateTimeOffset());
                            }

                            break;
                        }
                }
            }

            return input;
        }

        private static FluidValue Format(FilterArguments arguments, DateTimeOffset value)
        {
            var formatted = value.ToString(arguments.At(0).ToStringValue());

            return new StringValue(formatted);
        }
    }
}
