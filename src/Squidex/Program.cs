// ==========================================================================
//  Program.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.IO;
using Microsoft.AspNetCore.Hosting;

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
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
