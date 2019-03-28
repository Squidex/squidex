// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Infrastructure.Plugins
{
    public interface IPlugin
    {
        void ConfigureServices(IServiceCollection services, IConfiguration config);
    }
}
