// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Values;
using Newtonsoft.Json;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Templates.Extensions
{
    public sealed class StringFluidExtension : IFluidExtension
    {
        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            TemplateContext.GlobalFilters.AddFilter("escape", Escape);
            TemplateContext.GlobalFilters.AddFilter("slugify", Slugify);
            TemplateContext.GlobalFilters.AddFilter("trim", Trim);
        }

        public static FluidValue Slugify(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input is StringValue value)
            {
                var result = value.ToStringValue().Slugify();

                return FluidValue.Create(result);
            }

            return input;
        }

        public static FluidValue Escape(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var result = input.ToStringValue();

            result = JsonConvert.ToString(result);
            result = result[1..^1];

            return FluidValue.Create(result);
        }

        public static FluidValue Trim(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return FluidValue.Create(input.ToStringValue().Trim());
        }
    }
}
