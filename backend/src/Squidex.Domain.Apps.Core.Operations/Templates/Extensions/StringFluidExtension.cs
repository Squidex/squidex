// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Values;
using Squidex.Infrastructure;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Templates.Extensions;

public sealed class StringFluidExtension : IFluidExtension
{
    private static readonly FilterDelegate Slugify = (input, arguments, context) =>
    {
        if (input is StringValue value)
        {
            var result = value.ToStringValue().Slugify();

            return StringValue.Create(result);
        }

        return input;
    };

    private static readonly FilterDelegate Escape = (input, arguments, context) =>
    {
        return StringValue.Create(input.ToStringValue().JsonEscape());
    };

    private static readonly FilterDelegate Markdown2Text = (input, arguments, context) =>
    {
        return StringValue.Create(input.ToStringValue().Markdown2Text());
    };

    private static readonly FilterDelegate Html2Text = (input, arguments, context) =>
    {
        return StringValue.Create(input.ToStringValue().Html2Text());
    };

    private static readonly FilterDelegate Trim = (input, arguments, context) =>
    {
        return StringValue.Create(input.ToStringValue().Trim());
    };

    private static readonly FilterDelegate MD5 = (input, arguments, context) =>
    {
        return StringValue.Create(input.ToStringValue().ToMD5());
    };

    private static readonly FilterDelegate Sha256 = (input, arguments, context) =>
    {
        return StringValue.Create(input.ToStringValue().ToSha256());
    };

    public void RegisterLanguageExtensions(CustomFluidParser parser, TemplateOptions options)
    {
        options.Filters.AddFilter("escape", Escape);
        options.Filters.AddFilter("html2text", Html2Text);
        options.Filters.AddFilter("markdown2text", Markdown2Text);
        options.Filters.AddFilter("md5", MD5);
        options.Filters.AddFilter("sha256", Sha256);
        options.Filters.AddFilter("slugify", Slugify);
        options.Filters.AddFilter("trim", Trim);
    }
}
