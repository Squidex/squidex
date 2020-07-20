// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;

namespace Squidex.Domain.Apps.Core.Templates
{
    public interface IFluidExtension
    {
        void RegisterLanguageExtensions(FluidParserFactory factory)
        {
        }

        void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
        }

        void BeforeRun(TemplateContext templateContext)
        {
        }
    }
}
