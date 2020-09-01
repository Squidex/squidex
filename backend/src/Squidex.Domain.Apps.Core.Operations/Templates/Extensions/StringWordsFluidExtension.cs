// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Values;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Templates.Extensions
{
    public class StringWordsFluidExtension : IFluidExtension
    {
        private static readonly FilterDelegate WordCount = (input, arguments, context) =>
        {
            return FluidValue.Create(input.ToStringValue().WordCount());
        };

        private static readonly FilterDelegate CharacterCount = (input, arguments, context) =>
        {
            return FluidValue.Create(input.ToStringValue().CharacterCount());
        };

        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            TemplateContext.GlobalFilters.AddFilter("word_count", WordCount);
            TemplateContext.GlobalFilters.AddFilter("character_count", CharacterCount);
        }
    }
}
