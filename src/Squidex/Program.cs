// ==========================================================================
//  Program.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.IO;
using Microsoft.AspNetCore.Hosting;
using Squidex.Infrastructure.Log.Adapter;

namespace Squidex
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel(k => { k.AddServerHeader = false; })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureLogging(builder =>
                {
                    builder.AddSemanticLog();
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder.AddAppConfiguration(hostContext.HostingEnvironment.EnvironmentName, args);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddAppServices(context.Configuration);
                })
                .Build()
                .Run();
        }
    }
}
