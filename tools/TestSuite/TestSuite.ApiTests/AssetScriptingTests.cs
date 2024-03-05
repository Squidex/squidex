// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class AssetScriptingTests : IClassFixture<ClientFixture>
{
    public ClientFixture _ { get; }

    public AssetScriptingTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_disallow_jpeg_file_on_create()
    {
        var (client, _) = await _.PostAppAsync();

        // STEP 0: Configure script.
        var scriptRequest = new UpdateAssetScriptsDto
        {
            Create = @"
                if (ctx.command.mimeType == 'image/jpeg') {
                    disallow('We do not use jpeg anymore.');
                }"
        };

        await client.Apps.PutAssetScriptsAsync(scriptRequest);


        // STEP 1: Upload jpeg.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() => client.Assets.UploadFileAsync("Assets/logo-wide-rotated.jpg", "image/jpeg"));

        Assert.Contains("We do not use jpeg anymore.", ex.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_update_metadata_and_tags_on_create()
    {
        var (client, _) = await _.PostAppAsync();

        // STEP 0: Configure script.
        var scriptRequest = new UpdateAssetScriptsDto
        {
            Create = @"
                ctx.command.metadata['key1'] = 'value1';
                ctx.command.metadata['key2'] = 'value2';
                ctx.command.tags.add('tag1');
                ctx.command.tags.add('tag2');"
        };

        await client.Apps.PutAssetScriptsAsync(scriptRequest);


        // STEP 1: Upload jpeg.
        var asset = await client.Assets.UploadFileAsync("Assets/logo-wide-rotated.jpg", "image/jpeg");

        Assert.Equal("value1", asset.Metadata["key1"]);
        Assert.Equal("value2", asset.Metadata["key2"]);
        Assert.Contains("tag1", asset.Tags);
        Assert.Contains("tag2", asset.Tags);
    }

    [Fact]
    public async Task Should_disallow_jpeg_file_on_update()
    {
        var (client, _) = await _.PostAppAsync();

        // STEP 0: Configure script.
        var scriptRequest = new UpdateAssetScriptsDto
        {
            Update = @"
                if (ctx.command.mimeType == 'image/jpeg') {
                    disallow('We do not use jpeg anymore.');
                }"
        };

        await client.Apps.PutAssetScriptsAsync(scriptRequest);


        // STEP 1. Upload initial png.
        var asset_0 = await client.Assets.UploadFileAsync("Assets/logo-wide.png", "image/png");


        // STEP 2: Upload jpeg.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() => client.Assets.ReplaceFileAsync(asset_0.Id, "Assets/logo-wide-rotated.jpg", "image/jpeg"));

        Assert.Contains("We do not use jpeg anymore.", ex.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_update_metadata_on_update()
    {
        var (client, _) = await _.PostAppAsync();

        // STEP 0: Configure script.
        var scriptRequest = new UpdateAssetScriptsDto
        {
            Update = @"
                ctx.command.metadata['key1'] = 'value1';
                ctx.command.metadata['key2'] = 'value2';"
        };

        await client.Apps.PutAssetScriptsAsync(scriptRequest);


        // STEP 1. Upload initial png.
        var asset_0 = await client.Assets.UploadFileAsync("Assets/logo-wide.png", "image/png");


        // STEP 2: Upload jpeg.
        var asset = await client.Assets.ReplaceFileAsync(asset_0.Id, "Assets/logo-wide-rotated.jpg", "image/jpeg");

        Assert.Equal("value1", asset.Metadata["key1"]);
        Assert.Equal("value2", asset.Metadata["key2"]);
    }

    [Fact]
    public async Task Should_query_asset_with_scripting()
    {
        var (client, _) = await _.PostAppAsync();

        // STEP 0: Configure script.
        var scriptRequest = new UpdateAssetScriptsDto
        {
            Query = @"
                if (ctx.asset.mimeType == 'image/jpeg') {
                    disallow('We do not use jpeg anymore.');
                }"
        };

        await client.Apps.PutAssetScriptsAsync(scriptRequest);


        // STEP 1. Upload initial jpg.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() => client.Assets.UploadFileAsync("Assets/logo-wide-rotated.jpg", "image/jpeg"));

        Assert.Contains("We do not use jpeg anymore.", ex.ToString(), StringComparison.Ordinal);
    }
}
