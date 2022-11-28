// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Reactive.Linq;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Model;

namespace TestSuite.ApiTests;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

public class GraphQLSubscriptionTests : IClassFixture<ContentFixture>
{
    public ContentFixture _ { get; set; }

    public GraphQLSubscriptionTests(ContentFixture fixture)
    {
        _ = fixture;
    }

    private sealed class ContentChangesResult
    {
        public ContentChanges ContentChanges { get; set; }
    }

    private sealed class ContentChanges
    {
        public string Id { get; set; }
    }

    private sealed class AssetChangesResult
    {
        public AssetChanges AssetChanges { get; set; }
    }

    private sealed class AssetChanges
    {
        public string Id { get; set; }
    }

    [Fact]
    public async Task Should_listen_to_content_changes()
    {
        var client = await CreateClient();


        // STEP 1: Subscribe to changes.
        var contentChanges = new GraphQLRequest
        {
            Query = @"
                subscription {
                    contentChanges {
                        id
                    }
                }"
        };

        var contentId = Guid.NewGuid().ToString();

        var subscriptionStream
            = client.CreateSubscriptionStream<ContentChangesResult>(contentChanges);

        var publishedContent =
            subscriptionStream.Where(x => x.Data.ContentChanges.Id == contentId).Timeout(TimeSpan.FromSeconds(30))
                .FirstOrDefaultAsync();


        // STEP 2: Create Content.
        await _.Contents.CreateAsync(new TestEntityData(), new ContentCreateOptions { Id = contentId });


        // STEP 3: Wait for publication.
        var publishedResult = await publishedContent;

        Assert.Equal(contentId, publishedResult.Data.ContentChanges.Id);
    }

    [Fact]
    public async Task Should_listen_to_asset_changes()
    {
        var client = await CreateClient();


        // STEP 1: Subscribe to changes.
        var assetChanges = new GraphQLRequest
        {
            Query = @"
                subscription {
                    assetChanges {
                        id
                    }
                }"
        };

        var assetId = Guid.NewGuid().ToString();

        var subscriptionStream
            = client.CreateSubscriptionStream<AssetChangesResult>(assetChanges);

        var publishedAsset =
            subscriptionStream.Where(x => x.Data.AssetChanges.Id == assetId).Timeout(TimeSpan.FromSeconds(30))
                .FirstOrDefaultAsync();


        // STEP 2: Create asset.
        var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

        await using (fileParameter.Data)
        {
            await _.Assets.UploadAssetAsync(_.AppName, fileParameter, new AssetUploadOptions { Id = assetId });
        }

        // STEP 3: Wait for publication.
        var publishedResult = await publishedAsset;

        Assert.Equal(assetId, publishedResult.Data.AssetChanges.Id);
    }

    private async Task<GraphQLHttpClient> CreateClient()
    {
        var accessToken = await _.ClientManager.Options.Authenticator.GetBearerTokenAsync(_.AppName, default);

        var options = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(_.ClientManager.GenerateUrl($"/api/content/{_.AppName}/graphql?access_token={accessToken}"))
        };

        var client = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());

        await client.InitializeWebsocketConnection();

        return client;
    }
}
