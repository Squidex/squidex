using System;
using System.Threading.Tasks;
using DeploymentApp.Entities;
using DeploymentApp.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Squidex.ClientLibrary;

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

            var options = configuration.Get<AppOptions>();

            if (!options.Validate())
            {
                return -1;
            }

            try
            {
                var authenticator =
                    new CachingAuthenticator("TOKEN", new MemoryCache(Options.Create(new MemoryCacheOptions())),
                        new IcisAuthenticatorExtensions(
                            options.IdentityServer,
                            options.ClientId,
                            options.ClientSecret));

                var clientManager = new SquidexClientManager(options.Url, options.App, authenticator, true);

                await clientManager.CreateApp();

                await clientManager.UpsertSchema(Schemas.Commodity);
                await clientManager.UpsertSchema(Schemas.Region);
                await clientManager.UpsertSchema(Schemas.CommentaryType);
                await clientManager.UpsertSchema(Schemas.Commentary(options.Url));

                foreach (var role in Roles.All)
                {
                    await clientManager.UpsertRole(role);
                }

                foreach (var language in Languages.All)
                {
                    await clientManager.UpsertLanguage(language);
                }

                foreach (var contributor in Contributors.All)
                {
                    await clientManager.UpsertContributor(contributor);
                }

                foreach (var workflow in Workflows.All)
                {
                    await clientManager.UpsertWorkflow(workflow);
                }

                if (!options.SkipRules)
                {
                    foreach (var rule in Rules.AllKafkaRules)
                    {
                        await clientManager.UpsertKafkaRule(rule);
                    }
                }

                if (options.GenerateTestData)
                {
                    foreach (var (id, name) in TestData.CommentaryTypes)
                    {
                        await clientManager.CreateContentAsync("commentary-type", id, name);
                    }

                    foreach (var (id, name) in TestData.Regions)
                    {
                        await clientManager.CreateContentAsync("region", id, name);
                    }

                    foreach (var (id, name) in TestData.Commodities)
                    {
                        await clientManager.CreateContentAsync("commodity", id, name);
                    }
                }

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
