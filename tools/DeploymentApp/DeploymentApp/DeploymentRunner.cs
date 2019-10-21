using System.Threading.Tasks;
using DeploymentApp.Entities;
using DeploymentApp.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.ClientLibrary;

namespace DeploymentApp
{
    public static class DeploymentRunner
    {
        public static async Task RunAsync(DeploymentOptions options, ILogger logger)
        {
            var authenticator =
                new CachingAuthenticator("TOKEN", new MemoryCache(Options.Create(new MemoryCacheOptions())),
                    new IcisAuthenticatorExtensions(
                        options.IdentityServer,
                        options.ClientId,
                        options.ClientSecret));

            var clientManager = new SquidexClientManager(options.Url, options.App, authenticator, true);

            var client = new IcisClient(logger, clientManager);

            await client.CreateApp();

            await client.UpsertSchema(Schemas.Commodity);
            await client.UpsertSchema(Schemas.Region);
            await client.UpsertSchema(Schemas.Period);
            await client.UpsertSchema(Schemas.CommentaryType);
            await client.UpsertSchema(Schemas.Commentary());

            foreach (var role in Roles.All)
            {
                await client.UpsertRole(role);
            }

            foreach (var language in Languages.All)
            {
                await client.UpsertLanguage(language);
            }

            foreach (var contributor in Contributors.All)
            {
                await client.UpsertContributor(contributor);
            }

            foreach (var workflow in Workflows.All)
            {
                await client.UpsertWorkflow(workflow);
            }

            if (!options.SkipRules)
            {
                foreach (var rule in Rules.AllKafkaRules)
                {
                    await client.UpsertKafkaRule(rule);
                }
            }

            foreach (var (id, name) in TestData.CommentaryTypes)
            {
                await client.CreateIdDataAsync("commentary-type", id, name, false);
            }

            if (options.GenerateTestData)
            {

                foreach (var (id, name) in TestData.Regions)
                {
                    await client.CreateIdDataAsync("region", id, name, true);
                }

                foreach (var (id, name) in TestData.Commodities)
                {
                    await client.CreateIdDataAsync("commodity", id, name, true);
                }

                foreach (var (id, name) in TestData.Periods)
                {
                    await client.CreateIdDataAsync("period", id, name, true);
                }

                foreach (var (createdFor, commentaryType, commodity, region, body) in TestData.Commentaries)
                {
                    await client.CreateCommentaryAsync(createdFor, commodity, commentaryType, region, body);
                }
            }
        }
    }
}
