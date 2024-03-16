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
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Templates.Extensions
{
    public sealed class StringFluidExtension : IFluidExtension
    {
        private static readonly FilterDelegate Slugify = (input, arguments, context) =>
        {
            if (input is StringValue value)
            {
                var result = value.ToStringValue().Slugify();

                return FluidValue.Create(result);
            }

            return input;
        };

        private static readonly FilterDelegate Escape = (input, arguments, context) =>
        {
            var result = input.ToStringValue();

            result = JsonConvert.ToString(result);
            result = result[1..^1];

            return FluidValue.Create(result);
        };

        private static readonly FilterDelegate Markdown2Text = (input, arguments, context) =>
        {
            return FluidValue.Create(input.ToStringValue().Markdown2Text());
        };

        private static readonly FilterDelegate Html2Text = (input, arguments, context) =>
        {
            return FluidValue.Create(input.ToStringValue().Html2Text());
        };

        private static readonly FilterDelegate Trim = (input, arguments, context) =>
        {
            return FluidValue.Create(input.ToStringValue().Trim());
        };

        private static readonly FilterDelegate MD5 = (input, arguments, context) =>
        {
            return FluidValue.Create(input.ToStringValue().ToMD5());
        };

        private static readonly FilterDelegate Sha256 = (input, arguments, context) =>
        {
            return FluidValue.Create(input.ToStringValue().ToSha256());
        };

        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            TemplateContext.GlobalFilters.AddFilter("escape", Escape);
            TemplateContext.GlobalFilters.AddFilter("html2text", Html2Text);
            TemplateContext.GlobalFilters.AddFilter("markdown2text", Markdown2Text);
            TemplateContext.GlobalFilters.AddFilter("md5", MD5);
            TemplateContext.GlobalFilters.AddFilter("sha256", Sha256);
            TemplateContext.GlobalFilters.AddFilter("slugify", Slugify);
            TemplateContext.GlobalFilters.AddFilter("trim", Trim);
        }
    }
}
