using System;
using System.Threading.Tasks;
using DeploymentApp.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace DeploymentApp
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            string envName = Environment.GetEnvironmentVariable("Environment");
            string envConfigFile = $"./conf/appsettings.{envName}.json";
            string envSecretsFile = $"./secrets/appsettings.secrets.json";

            var configuration =
                new ConfigurationBuilder()
                    .AddJsonFile(envSecretsFile, true)
                    .AddJsonFile(envConfigFile, true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

            var options = configuration.Get<DeploymentOptions>();

            if (!options.Validate())
            {
                return -1;
            }

            try
            {
                await DeploymentRunner.RunAsync(options, new ConsoleLogger());

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to execute application: {0}", ex.Message);
                return -1;
            }
        }
    }
}
