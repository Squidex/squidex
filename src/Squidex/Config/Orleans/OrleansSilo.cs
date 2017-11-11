// ==========================================================================
//  OrleansSilo.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Orleans.Hosting;
using System;
using System.Threading.Tasks;

namespace Squidex.Config.Orleans
{
    public sealed class OrleansSilo
    {
        private readonly IServiceProvider serviceProvider;

        public Task RunAsync()
        {
            new SiloHostBuilder()
                .ConfigureLocalHostPrimarySilo(33333)
                .ConfigureSiloName("Squidex")
                .UseServiceProviderFactory()
        }
    }
}
