// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using Microsoft.AspNetCore.Hosting;
using Squidex.Infrastructure.Log.Adapter;

namespace Squidex
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel(k => { k.AddServerHeader = false; })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<WebStartup>()
                .ConfigureLogging(builder =>
                {
                    builder.AddSemanticLog();
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder.AddAppConfiguration(hostContext.HostingEnvironment.EnvironmentName, args);
                })
                .Build()
                .Run();
        }
    }
}
