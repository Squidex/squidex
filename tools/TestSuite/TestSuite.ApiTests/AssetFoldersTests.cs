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

[UsesVerify]
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
        // STEP 1: Create folder.
        var createRequest = new CreateAssetFolderDto
        {
            FolderName = Guid.NewGuid().ToString()
        };

        var folder = await _.Client.Assets.PostAssetFolderAsync(createRequest);

        Assert.Equal(createRequest.FolderName, folder.FolderName);

        await Verify(folder);
    }

    [Fact]
    public async Task Should_update_folder()
    {
        // STEP 1: Create folder.
        var createRequest = new CreateAssetFolderDto
        {
            FolderName = Guid.NewGuid().ToString()
        };

        var folder_0 = await _.Client.Assets.PostAssetFolderAsync(createRequest);


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
    public async Task Should_delete_folder()
    {
        // STEP 1: Create folder.
        var createRequest = new CreateAssetFolderDto
        {
            FolderName = Guid.NewGuid().ToString()
        };

        var folder_0 = await _.Client.Assets.PostAssetFolderAsync(createRequest);


        // STEP 2: Update folder
        await _.Client.Assets.DeleteAssetFolderAsync(folder_0.Id);

        // Should not return deleted folder.
        var folders = await _.Client.Assets.GetAssetFoldersAsync(folder_0.Id);

        Assert.DoesNotContain(folders.Items, x => x.Id == folder_0.Id);
    }

    [Fact]
    public async Task Should_create_and_query_nested_folders()
    {
        // STEP 1: Create folder.
        var createRequest = new CreateAssetFolderDto
        {
            FolderName = Guid.NewGuid().ToString()
        };

        var folder1 = await _.Client.Assets.PostAssetFolderAsync(createRequest);


        // STEP 2: Create nested folder.
        var createRequest2 = new CreateAssetFolderDto
        {
            FolderName = Guid.NewGuid().ToString(),
            // Create a nested asset folder.
            ParentId = folder1.Id
        };

        var folder2 = await _.Client.Assets.PostAssetFolderAsync(createRequest2);


        // STEP 3: Query by root id.
        var folders1 = await _.Client.Assets.GetAssetFoldersAsync(folder1.ParentId);

        Assert.Contains(folder1.Id, folders1.Items.Select(x => x.Id));


        // STEP 3: Query by nested id.
        var folders2 = await _.Client.Assets.GetAssetFoldersAsync(folder1.Id);

        Assert.Equal(new[] { folder2.Id }, folders2.Items.Select(x => x.Id));


        // STEP 3: Query all
        var folders3 = await _.Client.Assets.GetAssetFoldersAsync();

        Assert.Contains(folder1.Id, folders3.Items.Select(x => x.Id));
        Assert.Contains(folder2.Id, folders3.Items.Select(x => x.Id));
    }
}
