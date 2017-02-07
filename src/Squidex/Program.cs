// ==========================================================================
//  Program.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;

// ReSharper disable InvertIf

namespace Squidex
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel(k => { k.AddServerHeader = false; })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            if (args.Length > 0)
            {
                var commands = host.Services.GetService<IEnumerable<ICliCommand>>();

                foreach (var command in commands)
                {
                    if (string.Equals(args[0], command.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        command.Execute(args.Skip(1).ToArray());
                        return;
                    }
                }

                Console.WriteLine("Unknown command: {0}", args[0]);
            }
            else
            {
                host.Run();
            }
        }
    }
}
