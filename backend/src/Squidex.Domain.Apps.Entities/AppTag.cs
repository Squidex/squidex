// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Fluid.Tags;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Domain.Apps.Entities
{
    internal abstract class AppTag : ArgumentsTag
    {
        protected IAppProvider AppProvider { get; }

        protected AppTag(IServiceProvider serviceProvider)
        {
            AppProvider = serviceProvider.GetRequiredService<IAppProvider>();
        }
    }
}
