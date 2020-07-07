// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Values;

namespace Squidex.Domain.Apps.Core.Templates.Extensions
{
    public class StringWordsFluidExtension : IFluidExtension
    {
        private static readonly FilterDelegate WordCount = (input, arguments, context) =>
        {
            return FluidValue.Create(TextHelpers.WordCount(input.ToStringValue()));
        };

        private static readonly FilterDelegate CharacterCount = (input, arguments, context) =>
        {
            return FluidValue.Create(TextHelpers.CharacterCount(input.ToStringValue()));
        };

        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            TemplateContext.GlobalFilters.AddFilter("word_count", WordCount);
            TemplateContext.GlobalFilters.AddFilter("character_count", CharacterCount);
        }
    }
}
