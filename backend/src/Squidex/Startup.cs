﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Areas.Frontend;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Config.Authentication;
using Squidex.Config.Domain;
using Squidex.Config.Messaging;
using Squidex.Config.Web;
using Squidex.Pipeline.Plugins;
using Squidex.Web;
using Squidex.Web.Pipeline;

namespace Squidex;

public sealed class Startup(IConfiguration config)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddMemoryCache();
        services.AddHealthChecks();
        services.AddDefaultWebServices(config);
        services.AddDefaultForwardRules();

        // They must be called in this order.
        services.AddSquidexMvcWithPlugins(config);
        services.AddSquidexIdentity(config);
        services.AddSquidexIdentityServer();
        services.AddSquidexAuthentication(config);

        services.AddSquidexApps(config);
        services.AddSquidexAssetInfrastructure(config);
        services.AddSquidexAssets(config);
        services.AddSquidexBackups();
        services.AddSquidexCollaborations(config);
        services.AddSquidexCommands(config);
        services.AddSquidexContents(config);
        services.AddSquidexControllerServices(config);
        services.AddSquidexEventSourcing(config);
        services.AddSquidexFrontend();
        services.AddSquidexGraphQL();
        services.AddSquidexHealthChecks(config);
        services.AddSquidexHistory(config);
        services.AddSquidexImageResizing(config);
        services.AddSquidexInfrastructure(config);
        services.AddSquidexLocalization();
        services.AddSquidexMessaging(config);
        services.AddSquidexMigration(config);
        services.AddSquidexOpenApiSettings();
        services.AddSquidexQueries(config);
        services.AddSquidexRules(config);
        services.AddSquidexSchemas(config);
        services.AddSquidexSearch();
        services.AddSquidexSerializers();
        services.AddSquidexStoreServices(config);
        services.AddSquidexSubscriptions(config);
        services.AddSquidexTeams();
        services.AddSquidexTelemetry(config);
        services.AddSquidexTranslation(config);
        services.AddSquidexUsageTracking(config);

        // Run last to override and wrap existing services.
        services.AddSquidexPluginsPost(config);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseWebSockets();
        app.UseCookiePolicy();
        app.UseDefaultPathBase();
        app.UseDefaultForwardRules();
        app.UseSquidexLogging();
        app.UseSquidexLocalization();
        app.UseSquidexLocalCache();
        app.UseSquidexCors();
        app.UseSquidexCompression();
        app.UseSquidexHealthCheck();
        app.UseSquidexRobotsTxt();

        app.UseOpenApi(options =>
        {
            options.Path = "/api/swagger/v1/swagger.json";
        });

        app.UseWhen(c =>
            c.Request.Path.Value?.StartsWith(Constants.PrefixIdentityServer, StringComparison.OrdinalIgnoreCase) == true ||
            c.Request.Path.Value?.StartsWith(Constants.PrefixSignin, StringComparison.OrdinalIgnoreCase) == true,
            builder =>
            {
                builder.UseExceptionHandler("/identity-server/error");
            });

        app.UseWhen(c => c.Request.Path.StartsWithSegments(Constants.PrefixApi, StringComparison.OrdinalIgnoreCase), builder =>
        {
            builder.UseSquidexCacheKeys();
            builder.UseSquidexExceptionHandling();
            builder.UseSquidexUsage();
            builder.UseAccessTokenQueryString();
        });

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // Return a 404 for all unresolved api requests.
        app.Map(Constants.PrefixApi, builder =>
        {
            builder.Use404();
        });

        app.UseFrontend();
        app.UsePlugins();
    }
}
