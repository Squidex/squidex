// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Utils;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class AssetTests : IClassFixture<CreatedAppFixture>
{
    private ProgressHandler progress = new ProgressHandler();

    public CreatedAppFixture _ { get; }

    public AssetTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_upload_asset()
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");

        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var downloaded = await _.DownloadAsync(asset_1);

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded?.Length);
        }

        await Verify(asset_1);
    }

    [Fact]
    public async Task Should_upload_asset_using_tus()
    {
        // STEP 1: Create asset.
        var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

        await using (fileParameter.Data)
        {
            await _.Client.Assets.UploadAssetAsync(fileParameter, progress.AsOptions());
        }

        Assert.Null(progress.Exception);
        Assert.NotEmpty(progress.Progress);
        Assert.NotNull(progress.Asset);

        await using (var stream = new FileStream("Assets/SampleVideo_1280x720_1mb.mp4", FileMode.Open))
        {
            var downloaded = await _.DownloadAsync(progress.Asset);

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded?.Length);
        }
    }

    [Fact]
    public async Task Should_upload_asset_using_tus_in_chunks()
    {
        for (var i = 0; i < 5; i++)
        {
            // STEP 1: Create asset.
            progress = new ProgressHandler();

            var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

            await _.Client.Assets.UploadInChunksAsync(progress, fileParameter);

            Assert.Null(progress.Exception);
            Assert.NotEmpty(progress.Progress);
            Assert.NotNull(progress.Asset);
            Assert.True(progress.Uploads.Count > 1);

            await using (var stream = new FileStream("Assets/SampleVideo_1280x720_1mb.mp4", FileMode.Open))
            {
                var downloaded = await _.DownloadAsync(progress.Asset);

                // Should dowload with correct size.
                Assert.Equal(stream.Length, downloaded?.Length);
            }
        }
    }

    [Fact]
    public async Task Should_upload_asset_with_custom_id()
    {
        var id = Guid.NewGuid().ToString();

        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png", id: id);

        Assert.Equal(id, asset_1.Id);

        await Verify(asset_1);
    }

    [Fact]
    public async Task Should_upload_asset_with_custom_id_using_tus()
    {
        var id = Guid.NewGuid().ToString();

        // STEP 1: Create asset.
        var fileParameter = FileParameter.FromPath("Assets/logo-squared.png");

        await _.Client.Assets.UploadAssetAsync(fileParameter, progress.AsOptions(id));

        Assert.Equal(id, progress.Asset?.Id);
    }

    [Fact]
    public async Task Should_not_create_asset_with_custom_id_twice()
    {
        var id = Guid.NewGuid().ToString();

        // STEP 1: Create asset.
        await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png", id: id);


        // STEP 2: Create a new item with a custom id.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png", id: id);
        });

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task Should_not_create_very_big_asset()
    {
        // STEP 1: Create small asset.
        await _.Client.Assets.UploadRandomFileAsync(1_000_000);


        // STEP 2: Create big asset.
        var ex = await Assert.ThrowsAnyAsync<Exception>(() =>
        {
            return _.Client.Assets.UploadRandomFileAsync(10_000_000);
        });

        // Client library cannot catch this exception properly.
        Assert.True(ex is HttpRequestException || ex is SquidexException);
    }

    [Fact]
    public async Task Should_replace_asset()
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Reupload asset.
        var asset_2 = await _.Client.Assets.ReplaceFileAsync(asset_1.Id, "Assets/logo-wide.png", "image/png");

        await using (var stream = new FileStream("Assets/logo-wide.png", FileMode.Open))
        {
            var downloaded = await _.DownloadAsync(asset_2);

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded?.Length);
        }

        await Verify(asset_2);
    }

    [Fact]
    public async Task Should_replace_asset_using_tus()
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Reupload asset.
        var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

        await using (fileParameter.Data)
        {
            await _.Client.Assets.UploadAssetAsync(fileParameter, progress.AsOptions(asset_1.Id));
        }

        Assert.Null(progress.Exception);
        Assert.NotNull(progress.Asset);
        Assert.NotEmpty(progress.Progress);

        await using (var stream = new FileStream("Assets/SampleVideo_1280x720_1mb.mp4", FileMode.Open))
        {
            var downloaded = await _.DownloadAsync(progress.Asset);

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded?.Length);
        }
    }

    [Fact]
    public async Task Should_replace_asset_using_tus_in_chunks()
    {
        for (var i = 0; i < 1; i++)
        {
            // STEP 1: Create asset.
            var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


            // STEP 2: Reupload asset.
            progress = new ProgressHandler();

            var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

            await _.Client.Assets.UploadInChunksAsync(progress, fileParameter, asset_1.Id);

            Assert.Null(progress.Exception);
            Assert.NotEmpty(progress.Progress);
            Assert.NotNull(progress.Asset);
            Assert.True(progress.Uploads.Count > 1);

            await using (var stream = new FileStream("Assets/SampleVideo_1280x720_1mb.mp4", FileMode.Open))
            {
                var downloaded = await _.DownloadAsync(progress.Asset);

                // Should dowload with correct size.
                Assert.Equal(stream.Length, downloaded?.Length);
            }
        }
    }

    [Fact]
    public async Task Should_annotate_asset_in_parallel()
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 3: Make parallel upserts.
        await Parallel.ForEachAsync(Enumerable.Range(0, 20), async (i, ct) =>
        {
            try
            {
                var randomTag1 = $"tag_{Guid.NewGuid()}";
                var randomTag2 = $"tag_{Guid.NewGuid()}";

                var randomMetadataRequest = new AnnotateAssetDto
                {
                    Tags =
                    [
                        randomTag1,
                        randomTag2
                    ]
                };

                await _.Client.Assets.PutAssetAsync(asset_1.Id, randomMetadataRequest, ct);
            }
            catch (SquidexException ex) when (ex.StatusCode is 409 or 412)
            {
                return;
            }
        });


        // STEP 3: Make an normal update to ensure nothing is corrupt.
        var tag1 = $"tag_{Guid.NewGuid()}";
        var tag2 = $"tag_{Guid.NewGuid()}";

        var metadataRequest = new AnnotateAssetDto
        {
            Tags =
            [
                tag1,
                tag2
            ]
        };

        var asset_2 = await _.Client.Assets.PutAssetAsync(asset_1.Id, metadataRequest);


        // STEP 4: Check tags.
        var tags = await _.Client.Assets.PollTagAsync(tag1);

        Assert.Contains(tag1, tags);
        Assert.Contains(tag2, tags);

        Assert.Equal(1, tags[tag1]);
        Assert.Equal(1, tags[tag2]);

        await Verify(asset_2)
            .IgnoreMember<AssetDto>(x => x.Version)
            .IgnoreMember<AssetDto>(x => x.Tags);
    }

    [Theory]
    [InlineData(ContentStrategies.Annotate.Single)]
    [InlineData(ContentStrategies.Annotate.Bulk)]
    public async Task Should_annote_asset_file_name(ContentStrategies.Annotate strategy)
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Annotate file name.
        var fileNameRequest = new AnnotateAssetDto
        {
            FileName = "My Image"
        };

        await _.Client.Assets.AnnotateAsync(asset_1, fileNameRequest, strategy);

        var asset_2 = await _.Client.Assets.GetAssetAsync(asset_1.Id);

        // Should provide updated file name.
        Assert.Equal(fileNameRequest.FileName, asset_2.FileName);

        await Verify(asset_2).UseParameters(strategy);
    }

    [Theory]
    [InlineData(ContentStrategies.Annotate.Single)]
    [InlineData(ContentStrategies.Annotate.Bulk)]
    public async Task Should_annote_asset_metadata(ContentStrategies.Annotate strategy)
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Annotate metadata.
        var metadataRequest = new AnnotateAssetDto
        {
            Metadata = new Dictionary<string, object>
            {
                ["pw"] = 100L,
                ["ph"] = 20L
            }
        };

        await _.Client.Assets.AnnotateAsync(asset_1, metadataRequest, strategy);

        var asset_2 = await _.Client.Assets.GetAssetAsync(asset_1.Id);

        // Should provide metadata.
        Assert.Equal(metadataRequest.Metadata, asset_2.Metadata);

        await Verify(asset_2).UseParameters(strategy);
    }

    [Theory]
    [InlineData(ContentStrategies.Annotate.Single)]
    [InlineData(ContentStrategies.Annotate.Bulk)]
    public async Task Should_annote_asset_slug(ContentStrategies.Annotate strategy)
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Annotate slug.
        var slugRequest = new AnnotateAssetDto
        {
            Slug = "my-image"
        };

        await _.Client.Assets.AnnotateAsync(asset_1, slugRequest, strategy);

        var asset_2 = await _.Client.Assets.GetAssetAsync(asset_1.Id);

        // Should provide updated slug.
        Assert.Equal(slugRequest.Slug, asset_2.Slug);

        await Verify(asset_2).UseParameters(strategy);
    }

    [Theory]
    [InlineData(ContentStrategies.Annotate.Single)]
    [InlineData(ContentStrategies.Annotate.Bulk)]
    public async Task Should_annote_asset_tags(ContentStrategies.Annotate strategy)
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Annotate tags.
        var tagsRequest = new AnnotateAssetDto
        {
            Tags =
            [
                "tag1",
                "tag2"
            ]
        };

        await _.Client.Assets.AnnotateAsync(asset_1, tagsRequest, strategy);

        var asset_2 = await _.Client.Assets.GetAssetAsync(asset_1.Id);

        // Should provide updated tags.
        Assert.Equal(tagsRequest.Tags, asset_2.Tags);

        await Verify(asset_2).UseParameters(strategy);
    }

    [Theory]
    [InlineData(ContentStrategies.Annotate.Single)]
    [InlineData(ContentStrategies.Annotate.Bulk)]
    public async Task Should_protect_asset(ContentStrategies.Annotate strategy)
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Download asset.
        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var downloaded = await _.DownloadAsync(asset_1);

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded?.Length);
        }


        // STEP 4: Protect asset.
        var protectRequest = new AnnotateAssetDto
        {
            IsProtected = true
        };

        await _.Client.Assets.AnnotateAsync(asset_1, protectRequest, strategy);

        var asset_2 = await _.Client.Assets.GetAssetAsync(asset_1.Id);


        // STEP 5: Download asset with authentication.
        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var downloaded = new MemoryStream();

            using (var assetStream = await _.Client.Assets.GetAssetContentBySlugAsync(asset_2.Id, string.Empty))
            {
                await assetStream.Stream.CopyToAsync(downloaded);
            }

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded.Length);
        }


        // STEP 5: Download asset without key.
        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var ex = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            {
                return _.DownloadAsync(asset_1);
            });

            // Should return 403 when not authenticated.
            Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        }


        // STEP 6: Download asset without key and version.
        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var ex = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            {
                return _.DownloadAsync(asset_1, 0);
            });

            // Should return 403 when not authenticated.
            Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        }

        await Verify(asset_2).UseParameters(strategy);
    }

    [Fact]
    public async Task Should_protect_asset_with_script()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Create folder.
        var folderRequest = new CreateAssetFolderDto
        {
            FolderName = "folder"
        };

        var folder = await app.Assets.PostAssetFolderAsync(folderRequest);


        // STEP 2: Create asset.
        var asset_1 = await app.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png", parentId: folder.Id);


        // STEP 3: Download asset before script.
        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var downloaded = await _.DownloadAsync(asset_1);

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded?.Length);
        }


        // STEP 4: Protect asset using a script.
        var scriptsRequest = new UpdateAssetScriptsDto
        {
            Query = $@"
                if (ctx.assetId === '{asset_1.Id}') {{
                    disallow();
                }}"
        };

        await app.Apps.PutAssetScriptsAsync(scriptsRequest);


        // STEP 5: Download asset.
        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var ex = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            {
                return _.DownloadAsync(asset_1);
            });

            // Should return 403 from the script.
            Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        }

        await Verify(asset_1);
    }

    [Fact]
    public async Task Should_compute_blur_hash_script()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Create folder.
        var folderRequest = new CreateAssetFolderDto
        {
            FolderName = "folder"
        };

        var folder = await app.Assets.PostAssetFolderAsync(folderRequest);


        // STEP 2: Set script to calculate blur hash
        var scriptsRequest = new UpdateAssetScriptsDto
        {
            Create = @"
                if (ctx.asset.type === 'Image') {
                    getAssetBlurHash(ctx.asset, function (hash) {
                        ctx.command.metadata['blurHash'] = hash;
                    });
                }",
            Update = @"
                if (ctx.asset.type === 'Image') {
                    getAssetBlurHash(ctx.asset, function (hash) {
                        ctx.command.metadata['blurHash'] = hash;
                    });
                }"
        };

        await app.Apps.PutAssetScriptsAsync(scriptsRequest);


        // STEP 3: Create asset.
        var asset_1 = await app.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png", parentId: folder.Id);


        // STEP 4: Create asset.
        var asset_2 = await app.Assets.ReplaceFileAsync(asset_1.Id, "Assets/logo-wide.png", "image/png");

        Assert.NotNull(asset_1.Metadata["blurHash"]);
        Assert.NotNull(asset_2.Metadata["blurHash"]);
        Assert.NotEqual(asset_1.Metadata["blurHash"], asset_2.Metadata["blurHash"]);

        await Verify(asset_2);
    }

    [Fact]
    public async Task Should_query_asset_by_metadata()
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Query asset by pixel width.
        var assets_1 = await _.Client.Assets.GetAssetsAsync(new AssetQuery
        {
            Filter = "metadata/pixelWidth eq 600"
        });

        Assert.Contains(assets_1.Items, x => x.Id == asset_1.Id);


        // STEP 3: Add custom metadata.
        asset_1.Metadata["custom"] = "foo";

        await _.Client.Assets.PutAssetAsync(asset_1.Id, new AnnotateAssetDto
        {
            Metadata = asset_1.Metadata
        });


        // STEP 4: Query asset by custom metadata.
        var assets_2 = await _.Client.Assets.GetAssetsAsync(new AssetQuery
        {
            Filter = "metadata/custom eq 'foo'"
        });

        Assert.Contains(assets_2.Items, x => x.Id == asset_1.Id);
    }

    [Fact]
    public async Task Should_query_asset_by_root_folder()
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Query asset by root folder.
        var assets_1 = await _.Client.Assets.GetAssetsAsync(new AssetQuery
        {
            ParentId = Guid.Empty.ToString()
        });

        Assert.Contains(assets_1.Items, x => x.Id == asset_1.Id);
    }

    [Fact]
    public async Task Should_query_asset_by_subfolder()
    {
        // STEP 1: Create asset folder.
        var folderRequest = new CreateAssetFolderDto
        {
            FolderName = "sub"
        };

        var folder = await _.Client.Assets.PostAssetFolderAsync(folderRequest);


        // STEP 1: Create asset in folder.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png", parentId: folder.Id);


        // STEP 2: Query asset by root folder.
        var assets_1 = await _.Client.Assets.GetAssetsAsync(new AssetQuery
        {
            ParentId = folder.Id
        });

        Assert.Single(assets_1.Items, x => x.Id == asset_1.Id);
    }

    [Fact]
    public async Task Should_delete_recursively()
    {
        // STEP 1: Create asset folder.
        var createRequest1 = new CreateAssetFolderDto
        {
            FolderName = "folder1"
        };

        var folder_1 = await _.Client.Assets.PostAssetFolderAsync(createRequest1);


        // STEP 2: Create nested asset folder.
        var createRequest2 = new CreateAssetFolderDto
        {
            FolderName = "subfolder",
            // Reference the parent folder by Id, so it must exist first.
            ParentId = folder_1.Id
        };

        var folder_2 = await _.Client.Assets.PostAssetFolderAsync(createRequest2);


        // STEP 3: Create asset in folder.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png", null, folder_2.Id);


        // STEP 4: Create asset outside folder.
        var asset_2 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 5: Delete folder.
        await _.Client.Assets.DeleteAssetFolderAsync(folder_1.Id);

        // Ensure that asset in folder is deleted.
        Assert.True(await _.Client.Assets.PollForDeletionAsync(asset_1.Id));

        // Ensure that other asset is not deleted.
        Assert.NotNull(await _.Client.Assets.GetAssetAsync(asset_2.Id));
    }

    [Theory]
    [InlineData(ContentStrategies.Deletion.SingleSoft)]
    [InlineData(ContentStrategies.Deletion.SinglePermanent)]
    [InlineData(ContentStrategies.Deletion.BulkSoft)]
    [InlineData(ContentStrategies.Deletion.BulkPermanent)]
    public async Task Should_delete_asset(ContentStrategies.Deletion strategy)
    {
        // STEP 1: Create asset.
        var asset = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Delete asset.
        await _.Client.Assets.DeleteAsync(asset, strategy);

        // Should return 404 when asset deleted.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Client.Assets.GetAssetAsync(asset.Id);
        });

        Assert.Equal(404, ex.StatusCode);


        // STEP 3: Retrieve all items and ensure that the deleted item does not exist.
        var updated = await _.Client.Assets.GetAssetsAsync((AssetQuery?)null);

        Assert.DoesNotContain(updated.Items, x => x.Id == asset.Id);


        // STEP 4: Retrieve all deleted items and check if found.
        var deleted = await _.Client.Assets.GetAssetsAsync(new AssetQuery
        {
            Filter = "isDeleted eq true"
        });

        var permanent = strategy is ContentStrategies.Deletion.SinglePermanent or ContentStrategies.Deletion.BulkPermanent;

        Assert.Equal(!permanent, deleted.Items.Exists(x => x.Id == asset.Id));
    }

    [Theory]
    [InlineData(ContentStrategies.Deletion.SingleSoft)]
    [InlineData(ContentStrategies.Deletion.SinglePermanent)]
    [InlineData(ContentStrategies.Deletion.BulkSoft)]
    [InlineData(ContentStrategies.Deletion.BulkPermanent)]
    public async Task Should_recreate_deleted_asset(ContentStrategies.Deletion strategy)
    {
        // STEP 1: Create asset.
        var asset_1 = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Delete asset.
        await _.Client.Assets.DeleteAsync(asset_1, strategy);


        // STEP 3: Recreate asset.
        var asset_2 = await _.Client.Assets.UploadFileAsync("Assets/logo-wide.png", "image/png");

        Assert.NotEqual(asset_1.FileSize, asset_2.FileSize);
    }

    [Fact]
    public async Task Should_recover_deleted_asset()
    {
        // STEP 0: Create app.
        var (client, _) = await _.PostAppAsync();


        // STEP 1: Create asset.
        var asset_1 = await client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Delete asset.
        await client.Assets.DeleteAssetAsync(asset_1.Id);


        // STEP 3: Query and recreate asset.
        var assets = await client.Assets.GetAssetsAsync(new AssetQuery
        {
            Query = new
            {
                filter = new
                {
                    path = "isDeleted",
                    op = "eq",
                    value = true,
                }
            }
        });

        Assert.NotEmpty(assets.Items);

        foreach (var asset in assets.Items)
        {
            var content = await client.Assets.GetAssetContentBySlugAsync(asset.Id, string.Empty, deleted: true);

            await client.Assets.PostAssetAsync(id: asset.Id, file: new FileParameter(content.Stream, asset.FileName, asset.MimeType));
        }


        // STEP 4: Query recreated asset.
        var asset_2 = await client.Assets.GetAssetAsync(asset_1.Id);

        Assert.NotNull(asset_2);
    }

    [Fact]
    public async Task Should_rename_tag()
    {
        // STEP 0: Create app.
        var (client, _) = await _.PostAppAsync();


        // STEP 1: Create asset.
        await client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Rename tag.
        var renameRequest = new RenameTagDto
        {
            TagName = "pngs"
        };

        await client.Assets.PutTagAsync("type/png", renameRequest);


        // STEP 2: Create asset.
        var asset2 = await client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");

        Assert.Contains("pngs", asset2.Tags);
        Assert.DoesNotContain("type/png", asset2.Tags);
    }
}
