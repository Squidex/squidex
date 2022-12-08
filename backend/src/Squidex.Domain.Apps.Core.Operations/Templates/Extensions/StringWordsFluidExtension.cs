// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Values;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Templates.Extensions;

public class StringWordsFluidExtension : IFluidExtension
{
    private static readonly FilterDelegate WordCount = (input, arguments, context) =>
    {
        return NumberValue.Create(input.ToStringValue().WordCount());
    };

    private static readonly FilterDelegate CharacterCount = (input, arguments, context) =>
    {
        return NumberValue.Create(input.ToStringValue().CharacterCount());
    };

    public void RegisterLanguageExtensions(CustomFluidParser parser, TemplateOptions options)
    {
        options.Filters.AddFilter("word_count", WordCount);
        options.Filters.AddFilter("character_count", CharacterCount);
    }
}
