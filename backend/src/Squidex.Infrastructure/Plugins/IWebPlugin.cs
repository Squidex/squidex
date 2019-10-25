// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;

namespace Squidex.Infrastructure.Plugins
{
    public interface IWebPlugin : IPlugin
    {
        void ConfigureBefore(IApplicationBuilder app);

        void ConfigureAfter(IApplicationBuilder app);
    }
}
