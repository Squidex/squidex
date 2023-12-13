// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.Text;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class AtlasTextIndexFixture : IAsyncLifetime
{
    public AtlasTextIndex Index { get; }

    public AtlasTextIndexFixture()
    {
        TestUtils.SetupBson();

        var mongoClient = MongoClientFactory.Create(TestConfig.Configuration["atlas:configuration"]!);
        var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["atlas:database"]!);

        var options = TestConfig.Configuration.GetSection("atlas").Get<AtlasOptions>()!;

        var services =
            new ServiceCollection()
                .AddSingleton(Options.Create(options))
                .AddSingleton(mongoClient)
                .AddSingleton(mongoDatabase)
                .AddHttpClient("Atlas", options =>
                {
                    options.BaseAddress = new Uri("https://cloud.mongodb.com/");
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler
                    {
                        Credentials = new NetworkCredential(options.PublicKey, options.PrivateKey, "cloud.mongodb.com")
                    };
                }).Services
                .BuildServiceProvider();

        Index = services.GetRequiredService<AtlasTextIndex>();
    }

    public Task InitializeAsync()
    {
        return Index.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
