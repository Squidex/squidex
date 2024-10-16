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

public class AssetFoldersTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; }

    public AssetFoldersTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_folder()
    {
        var name = Guid.NewGuid().ToString();

        // STEP 1: Create folder.
        var folder = await CreateFolderAsync(name, null);

        Assert.Equal(name, folder.FolderName);

        await Verify(folder);
    }

    [Fact]
    public async Task Should_update_folder()
    {
        // STEP 1: Create folder.
        var folder_0 = await CreateFolderAsync(Guid.NewGuid().ToString());


        // STEP 2: Update folder
        var updateRequest = new RenameAssetFolderDto
        {
            FolderName = Guid.NewGuid().ToString()
        };

        var folder_1 = await _.Client.Assets.PutAssetFolderAsync(folder_0.Id, updateRequest);

        Assert.Equal(updateRequest.FolderName, folder_1.FolderName);

        await Verify(folder_1);
    }

    [Fact]
    public async Task Should_move_folder()
    {
        // STEP 1: Create folders.
        var folder1 = await CreateFolderAsync(Guid.NewGuid().ToString());
        var folder2 = await CreateFolderAsync(Guid.NewGuid().ToString());


        // STEP 2: Update folder
        var moveRequest = new MoveAssetFolderDto
        {
            ParentId = folder1.Id
        };

        var folder2_1 = await _.Client.Assets.PutAssetFolderParentAsync(folder2.Id, moveRequest);

        Assert.Equal(folder1.Id, folder2_1.ParentId);
    }

    [Theory]
    [InlineData(ContentStrategies.Move.Single)]
    [InlineData(ContentStrategies.Move.Bulk)]
    public async Task Should_move_asset(ContentStrategies.Move strategy)
    {
        // STEP 1: Create folder.
        var folder1 = await CreateFolderAsync(Guid.NewGuid().ToString());


        // STEP 2: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Update folder
        await _.Client.Assets.MoveAsync(asset_1, folder1, strategy);

        var asset_2 = await _.Client.Assets.GetAssetAsync(asset_1.Id);

        Assert.Equal(folder1.Id, asset_2.ParentId);
    }

    [Fact]
    public async Task Should_not_move_folder_to_own_child()
    {
        // STEP 1: Create folders.
        var folder1 = await CreateFolderAsync(Guid.NewGuid().ToString());
        var folder2 = await CreateFolderAsync(Guid.NewGuid().ToString(), folder1.Id);


        // STEP 2: Update folder
        var moveRequest = new MoveAssetFolderDto
        {
            ParentId = folder2.Id
        };

        await Assert.ThrowsAnyAsync<SquidexException>(() => _.Client.Assets.PutAssetFolderParentAsync(folder1.Id, moveRequest));
    }

    [Fact]
    public async Task Should_delete_folder()
    {
        // STEP 1: Create folder.
        var folder_0 = await CreateFolderAsync(Guid.NewGuid().ToString(), null);


        // STEP 2: Update folder
        await _.Client.Assets.DeleteAssetFolderAsync(folder_0.Id);

        // Should not return deleted folder.
        var folders = await _.Client.Assets.GetAssetFoldersAsync(folder_0.Id);

        Assert.DoesNotContain(folders.Items, x => x.Id == folder_0.Id);
    }

    [Fact]
    public async Task Should_create_and_query_nested_folders()
    {
        // STEP 0: Create folders.
        var folder1 = await CreateFolderAsync(Guid.NewGuid().ToString());
        var folder2 = await CreateFolderAsync(Guid.NewGuid().ToString(), folder1.Id);


        // STEP 1: Query by root id.
        var folders1 = await _.Client.Assets.GetAssetFoldersAsync(folder1.ParentId);

        Assert.Contains(folder1.Id, folders1.Items.Select(x => x.Id));


        // STEP 3: Query by nested id.
        var folders2 = await _.Client.Assets.GetAssetFoldersAsync(folder1.Id);

        Assert.Equal([folder2.Id], folders2.Items.Select(x => x.Id));


        // STEP 3: Query all
        var folders3 = await _.Client.Assets.GetAssetFoldersAsync();

        Assert.Contains(folder1.Id, folders3.Items.Select(x => x.Id));
        Assert.Contains(folder2.Id, folders3.Items.Select(x => x.Id));
    }

    private async Task<AssetFolderDto> CreateFolderAsync(string name, string? parentId = null)
    {
        var createRequest = new CreateAssetFolderDto
        {
            FolderName = name,
            // Create a nested asset folder.
            ParentId = parentId
        };

        var folder = await _.Client.Assets.PostAssetFolderAsync(createRequest);

        return folder;
    }
}
