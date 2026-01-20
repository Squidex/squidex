// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public sealed class ContentUserTests(CreatedAppFixture fixture) : IClassFixture<CreatedAppFixture>
{
    private readonly string schemaName = $"schema-{Guid.NewGuid()}";

    public CreatedAppFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_login_with_user_credentials()
    {
        var apiKey = Guid.NewGuid().ToString();

        // STEP 1: Create schema.
        var createSchemaRequest = new CreateSchemaDto
        {
            Name = schemaName,
            Fields =
            [
                new UpsertSchemaFieldDto
                {
                    Name = "userInfo",
                    Properties = new UserInfoFieldPropertiesDto(),
                },
            ],
            IsPublished = true,
        };

        await _.Client.Schemas.PostSchemaAsync(createSchemaRequest);


        // STEP 2: Create user.
        var client = _.Client.DynamicContents(schemaName);

        await client.CreateAsync(
            new DynamicData
            {
                ["userInfo"] = new JObject
                {
                    ["iv"] = new JObject
                    {
                        ["role"] = "Reader",
                        // This API key is used for authentication later.
                        ["apiKey"] = apiKey,
                    },
                },
            },
            ContentCreateOptions.AsPublish);

        // STEP 3: Login.
        var apiKeyClient = new SquidexClient(new SquidexOptions
        {
            Url = _.Url,
            ApiKey = apiKey,
            ClientId = null!,
            ClientSecret = null!,
            AppName = _.AppName,
        });

        var apiKeyContents = apiKeyClient.DynamicContents(schemaName);

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    await apiKeyContents.GetAsync(ct: cts.Token);
                    return;
                }
                catch (SquidexException ex) when (ex.StatusCode == 401)
                {
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        Assert.Fail();
    }
}
