// ==========================================================================
//  WebpackRunner.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

// ReSharper disable ConvertToConstant.Local

namespace PinkParrot.Pipeline
{
    public sealed class WebpackRunner
    {
        private const string WebpackDevServer = "webpack-dev-server";

        private readonly ILoggerFactory loggerFactory;
        private readonly IApplicationLifetime lifetime;
        private Process process;

        public WebpackRunner(ILoggerFactory loggerFactory, IApplicationLifetime lifetime)
        {
            this.loggerFactory = loggerFactory;

            this.lifetime = lifetime;
        }

        public void Execute()
        {
            if (process != null)
            {
                return;
            }

            var logger = loggerFactory.CreateLogger(WebpackDevServer);

            EnsuereNodeModluesInstalled(logger);

            logger.LogInformation($"{WebpackDevServer} Execution started");

            var app = GetNodeExecutable(WebpackDevServer);
            var args = "--inline --hot --port 3000";

            process = Process.Start(new ProcessStartInfo
            {
                FileName = app, Arguments = args, UseShellExecute = false
            });

            lifetime.ApplicationStopping.Register(OnShutdown);

            logger.LogInformation($"{WebpackDevServer} started successfully");
        }

        private void OnShutdown()
        {
            process?.Kill();
            process = null;
        }

        private static void EnsuereNodeModluesInstalled(ILogger logger)
        {
            logger.LogInformation("Verifying required tools are installed");

            if (!File.Exists(GetNodeExecutable(WebpackDevServer)))
            {
                logger.LogError("webpack-dev-server is not installed. Please install it by executing npm i webpack-dev-server");
            }

            logger.LogInformation("All node modules are properly installed");
        }

        private static string GetNodeExecutable(string module)
        {
            var executablePath = Path.Combine(Directory.GetCurrentDirectory(), "node_modules", ".bin", module);

            var osEnVariable = Environment.GetEnvironmentVariable("OS");

            if (!string.IsNullOrEmpty(osEnVariable) && 
                 string.Equals(osEnVariable, "Windows_NT", StringComparison.OrdinalIgnoreCase))
            {
                executablePath += ".cmd";
            }

            return executablePath;
        }
    }
}
