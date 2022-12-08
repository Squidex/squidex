// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Squidex.Assets;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

[UsesVerify]
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
        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");

        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var downloaded = await _.DownloadAsync(asset_1);

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded.Length);
        }

        await Verify(asset_1);
    }

    [Fact]
    public async Task Should_upload_asset_using_tus()
    {
        // STEP 1: Create asset
        var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

        await using (fileParameter.Data)
        {
            await _.Assets.UploadAssetAsync(_.AppName, fileParameter, progress.AsOptions());
        }

        Assert.Null(progress.Exception);
        Assert.NotEmpty(progress.Progress);
        Assert.NotNull(progress.Asset);

        await using (var stream = new FileStream("Assets/SampleVideo_1280x720_1mb.mp4", FileMode.Open))
        {
            var downloaded = await _.DownloadAsync(progress.Asset);

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded.Length);
        }
    }

    [Fact]
    public async Task Should_upload_asset_using_tus_in_chunks()
    {
        for (var i = 0; i < 5; i++)
        {
            // STEP 1: Create asset
            progress = new ProgressHandler();

            var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

            await UploadInChunksAsync(fileParameter);

            Assert.Null(progress.Exception);
            Assert.NotEmpty(progress.Progress);
            Assert.NotNull(progress.Asset);
            Assert.True(progress.Uploads.Count > 1);

            await using (var stream = new FileStream("Assets/SampleVideo_1280x720_1mb.mp4", FileMode.Open))
            {
                var downloaded = await _.DownloadAsync(progress.Asset);

                // Should dowload with correct size.
                Assert.Equal(stream.Length, downloaded.Length);
            }
        }
    }

    [Fact]
    public async Task Should_upload_asset_with_custom_id()
    {
        var id = Guid.NewGuid().ToString();

        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png", id: id);

        Assert.Equal(id, asset_1.Id);

        await Verify(asset_1);
    }

    [Fact]
    public async Task Should_upload_asset_with_custom_id_using_tus()
    {
        var id = Guid.NewGuid().ToString();

        // STEP 1: Create asset
        var fileParameter = FileParameter.FromPath("Assets/logo-squared.png");

        await _.Assets.UploadAssetAsync(_.AppName, fileParameter, progress.AsOptions(id));

        Assert.Equal(id, progress.Asset?.Id);
    }

    [Fact]
    public async Task Should_not_create_asset_with_custom_id_twice()
    {
        var id = Guid.NewGuid().ToString();

        // STEP 1: Create asset
        await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png", id: id);


        // STEP 2: Create a new item with a custom id.
        var ex = await Assert.ThrowsAnyAsync<SquidexManagementException>(() =>
        {
            return _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png", id: id);
        });

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task Should_not_create_very_big_asset()
    {
        // STEP 1: Create small asset
        await _.Assets.UploadRandomFileAsync(_.AppName, 1_000_000);


        // STEP 2: Create big asset
        var ex = await Assert.ThrowsAnyAsync<Exception>(() =>
        {
            return _.Assets.UploadRandomFileAsync(_.AppName, 10_000_000);
        });

        // Client library cannot catch this exception properly.
        Assert.True(ex is HttpRequestException || ex is SquidexManagementException);
    }

    [Fact]
    public async Task Should_replace_asset()
    {
        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 2: Reupload asset
        var asset_2 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-wide.png", asset_1);

        await using (var stream = new FileStream("Assets/logo-wide.png", FileMode.Open))
        {
            var downloaded = await _.DownloadAsync(asset_2);

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded.Length);
        }

        await Verify(asset_2);
    }

    [Fact]
    public async Task Should_replace_asset_using_tus()
    {
        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 2: Reupload asset
        var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

        await using (fileParameter.Data)
        {
            await _.Assets.UploadAssetAsync(_.AppName, fileParameter, progress.AsOptions(asset_1.Id));
        }

        Assert.Null(progress.Exception);
        Assert.NotNull(progress.Asset);
        Assert.NotEmpty(progress.Progress);

        await using (var stream = new FileStream("Assets/SampleVideo_1280x720_1mb.mp4", FileMode.Open))
        {
            var downloaded = await _.DownloadAsync(progress.Asset);

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded.Length);
        }
    }

    [Fact]
    public async Task Should_replace_asset_using_tus_in_chunks()
    {
        for (var i = 0; i < 1; i++)
        {
            // STEP 1: Create asset
            var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


            // STEP 2: Reupload asset
            progress = new ProgressHandler();

            var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

            await UploadInChunksAsync(fileParameter, asset_1.Id);

            Assert.Null(progress.Exception);
            Assert.NotEmpty(progress.Progress);
            Assert.NotNull(progress.Asset);
            Assert.True(progress.Uploads.Count > 1);

            await using (var stream = new FileStream("Assets/SampleVideo_1280x720_1mb.mp4", FileMode.Open))
            {
                var downloaded = await _.DownloadAsync(progress.Asset);

                // Should dowload with correct size.
                Assert.Equal(stream.Length, downloaded.Length);
            }
        }
    }

    [Fact]
    public async Task Should_annote_asset_file_name()
    {
        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 2: Annotate file name.
        var fileNameRequest = new AnnotateAssetDto
        {
            FileName = "My Image"
        };

        var asset_2 = await _.Assets.PutAssetAsync(_.AppName, asset_1.Id, fileNameRequest);

        // Should provide updated file name.
        Assert.Equal(fileNameRequest.FileName, asset_2.FileName);

        await Verify(asset_2);
    }

    [Fact]
    public async Task Should_annote_asset_metadata()
    {
        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 2: Annotate metadata.
        var metadataRequest = new AnnotateAssetDto
        {
            Metadata = new Dictionary<string, object>
            {
                ["pw"] = 100L,
                ["ph"] = 20L
            }
        };

        var asset_2 = await _.Assets.PutAssetAsync(_.AppName, asset_1.Id, metadataRequest);

        // Should provide metadata.
        Assert.Equal(metadataRequest.Metadata, asset_2.Metadata);

        await Verify(asset_2);
    }

    [Fact]
    public async Task Should_annote_asset_slug()
    {
        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 2: Annotate slug.
        var slugRequest = new AnnotateAssetDto
        {
            Slug = "my-image"
        };

        var asset_2 = await _.Assets.PutAssetAsync(_.AppName, asset_1.Id, slugRequest);

        // Should provide updated slug.
        Assert.Equal(slugRequest.Slug, asset_2.Slug);

        await Verify(asset_2);
    }

    [Fact]
    public async Task Should_annote_asset_tags()
    {
        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 2: Annotate tags.
        var tagsRequest = new AnnotateAssetDto
        {
            Tags = new List<string>
            {
                "tag1",
                "tag2"
            }
        };

        var asset_2 = await _.Assets.PutAssetAsync(_.AppName, asset_1.Id, tagsRequest);

        // Should provide updated tags.
        Assert.Equal(tagsRequest.Tags, asset_2.Tags);

        await Verify(asset_2);
    }

    [Fact]
    public async Task Should_annotate_asset_in_parallel()
    {
        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 3: Make parallel upserts.
        await Parallel.ForEachAsync(Enumerable.Range(0, 20), async (i, ct) =>
        {
            try
            {
                var randomTag1 = $"tag_{Guid.NewGuid()}";
                var randomTag2 = $"tag_{Guid.NewGuid()}";

                var randomMetadataRequest = new AnnotateAssetDto
                {
                    Tags = new List<string>
                    {
                        randomTag1,
                        randomTag2
                    }
                };

                await _.Assets.PutAssetAsync(_.AppName, asset_1.Id, randomMetadataRequest);
            }
            catch (SquidexManagementException ex) when (ex.StatusCode is 409 or 412)
            {
                return;
            }
        });


        // STEP 3: Make an normal update to ensure nothing is corrupt.
        var tag1 = $"tag_{Guid.NewGuid()}";
        var tag2 = $"tag_{Guid.NewGuid()}";

        var metadataRequest = new AnnotateAssetDto
        {
            Tags = new List<string>
            {
                tag1,
                tag2
            }
        };

        var asset_2 = await _.Assets.PutAssetAsync(_.AppName, asset_1.Id, metadataRequest);


        // STEP 4: Check tags
        var tags = await _.Assets.WaitForTagsAsync(_.AppName, tag1, TimeSpan.FromMinutes(2));

        Assert.Contains(tag1, tags);
        Assert.Contains(tag2, tags);
        Assert.Equal(1, tags[tag1]);
        Assert.Equal(1, tags[tag2]);

        await Verify(asset_2)
            .IgnoreMember<AssetDto>(x => x.Version)
            .IgnoreMember<AssetDto>(x => x.Tags);
    }

    [Fact]
    public async Task Should_protect_asset()
    {
        var fileName = $"{Guid.NewGuid()}.png";

        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 2: Download asset
        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var downloaded = await _.DownloadAsync(asset_1);

            // Should dowload with correct size.
            Assert.Equal(stream.Length, downloaded.Length);
        }


        // STEP 4: Protect asset
        var protectRequest = new AnnotateAssetDto
        {
            IsProtected = true
        };

        var asset_2 = await _.Assets.PutAssetAsync(_.AppName, asset_1.Id, protectRequest);


        // STEP 5: Download asset with authentication.
        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var downloaded = new MemoryStream();

            using (var assetStream = await _.Assets.GetAssetContentBySlugAsync(_.AppName, asset_2.Id, string.Empty))
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

        await Verify(asset_2);
    }

    [Fact]
    public async Task Should_query_asset_by_metadata()
    {
        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 2: Query asset by pixel width.
        var assets_1 = await _.Assets.GetAssetsAsync(_.AppName, new AssetQuery
        {
            Filter = "metadata/pixelWidth eq 600"
        });

        Assert.Contains(assets_1.Items, x => x.Id == asset_1.Id);


        // STEP 3: Add custom metadata.
        asset_1.Metadata["custom"] = "foo";

        await _.Assets.PutAssetAsync(_.AppName, asset_1.Id, new AnnotateAssetDto
        {
            Metadata = asset_1.Metadata
        });


        // STEP 4: Query asset by custom metadata
        var assets_2 = await _.Assets.GetAssetsAsync(_.AppName, new AssetQuery
        {
            Filter = "metadata/custom eq 'foo'"
        });

        Assert.Contains(assets_2.Items, x => x.Id == asset_1.Id);
    }

    [Fact]
    public async Task Should_query_asset_by_root_folder()
    {
        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 2: Query asset by root folder.
        var assets_1 = await _.Assets.GetAssetsAsync(_.AppName, new AssetQuery
        {
            ParentId = Guid.Empty.ToString()
        });

        Assert.Contains(assets_1.Items, x => x.Id == asset_1.Id);
    }

    [Fact]
    public async Task Should_query_asset_by_subfolder()
    {
        // STEP 1: Create asset folder
        var folderRequest = new CreateAssetFolderDto
        {
            FolderName = "sub"
        };

        var folder = await _.Assets.PostAssetFolderAsync(_.AppName, folderRequest);


        // STEP 1: Create asset in folder
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png", parentId: folder.Id);


        // STEP 2: Query asset by root folder.
        var assets_1 = await _.Assets.GetAssetsAsync(_.AppName, new AssetQuery
        {
            ParentId = folder.Id
        });

        Assert.Single(assets_1.Items, x => x.Id == asset_1.Id);
    }

    [Fact]
    public async Task Should_delete_recursively()
    {
        // STEP 1: Create asset folder
        var createRequest1 = new CreateAssetFolderDto
        {
            FolderName = "folder1"
        };

        var folder_1 = await _.Assets.PostAssetFolderAsync(_.AppName, createRequest1);


        // STEP 2: Create nested asset folder
        var createRequest2 = new CreateAssetFolderDto
        {
            FolderName = "subfolder",
            // Reference the parent folder by Id, so it must exist first.
            ParentId = folder_1.Id
        };

        var folder_2 = await _.Assets.PostAssetFolderAsync(_.AppName, createRequest2);


        // STEP 3: Create asset in folder
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png", null, folder_2.Id);


        // STEP 4: Create asset outside folder
        var asset_2 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 5: Delete folder.
        await _.Assets.DeleteAssetFolderAsync(_.AppName, folder_1.Id);

        // Ensure that asset in folder is deleted.
        Assert.True(await _.Assets.WaitForDeletionAsync(_.AppName, asset_1.Id, TimeSpan.FromSeconds(30)));

        // Ensure that other asset is not deleted.
        Assert.NotNull(await _.Assets.GetAssetAsync(_.AppName, asset_2.Id));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_delete_asset(bool permanent)
    {
        // STEP 1: Create asset
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 2: Delete asset
        await _.Assets.DeleteAssetAsync(_.AppName, asset.Id, permanent: permanent);

        // Should return 404 when asset deleted.
        var ex = await Assert.ThrowsAnyAsync<SquidexManagementException>(() =>
        {
            return _.Assets.GetAssetAsync(_.AppName, asset.Id);
        });

        Assert.Equal(404, ex.StatusCode);


        // STEP 3: Retrieve all items and ensure that the deleted item does not exist.
        var updated = await _.Assets.GetAssetsAsync(_.AppName, (AssetQuery)null);

        Assert.DoesNotContain(updated.Items, x => x.Id == asset.Id);


        // STEP 4: Retrieve all deleted items and check if found.
        var deleted = await _.Assets.GetAssetsAsync(_.AppName, new AssetQuery
        {
            Filter = "isDeleted eq true"
        });

        Assert.Equal(!permanent, deleted.Items.Any(x => x.Id == asset.Id));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_recreate_deleted_asset(bool permanent)
    {
        // STEP 1: Create asset
        var asset_1 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-squared.png", "image/png");


        // STEP 2: Delete asset
        await _.Assets.DeleteAssetAsync(_.AppName, asset_1.Id, permanent: permanent);


        // STEP 3: Recreate asset
        var asset_2 = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-wide.png", "image/png");

        Assert.NotEqual(asset_1.FileSize, asset_2.FileSize);
    }

    private async Task UploadInChunksAsync(FileParameter fileParameter, string id = null)
    {
        var pausingStream = new PauseStream(fileParameter.Data, 0.25);
        var pausingFile = new FileParameter(pausingStream, fileParameter.FileName, fileParameter.ContentType)
        {
            ContentLength = fileParameter.Data.Length
        };

        await using (pausingFile.Data)
        {
            using var cts = new CancellationTokenSource(5000);

            while (progress.Asset == null && progress.Exception == null && !cts.IsCancellationRequested)
            {
                pausingStream.Reset();

                await _.Assets.UploadAssetAsync(_.AppName, pausingFile, progress.AsOptions(id), cts.Token);
                progress.Uploaded();
            }
        }
    }

    public class ProgressHandler : IAssetProgressHandler
    {
        public string FileId { get; private set; } = Guid.NewGuid().ToString();

        public List<int> Progress { get; } = new List<int>();

        public List<int> Uploads { get; } = new List<int>();

        public Exception Exception { get; private set; }

        public AssetDto Asset { get; private set; }

        public AssetUploadOptions AsOptions(string id = null)
        {
            var options = default(AssetUploadOptions);
            options.ProgressHandler = this;
            options.FileId = FileId;
            options.Id = id;

            return options;
        }

        public void Uploaded()
        {
            Uploads.Add(Progress.LastOrDefault());
        }

        public Task OnCompletedAsync(AssetUploadCompletedEvent @event,
            CancellationToken ct)
        {
            Asset = @event.Asset;
            return Task.CompletedTask;
        }

        public Task OnCreatedAsync(AssetUploadCreatedEvent @event,
            CancellationToken ct)
        {
            FileId = @event.FileId;
            return Task.CompletedTask;
        }

        public Task OnProgressAsync(AssetUploadProgressEvent @event,
            CancellationToken ct)
        {
            Progress.Add(@event.Progress);
            return Task.CompletedTask;
        }

        public Task OnFailedAsync(AssetUploadExceptionEvent @event,
            CancellationToken ct)
        {
            Exception = @event.Exception;
            return Task.CompletedTask;
        }
    }

    public class PauseStream : DelegateStream
    {
        private readonly int maxLength;
        private long totalRead;
        private long totalRemaining;
        private long seekStart;

        public override long Length
        {
            get => Math.Min(maxLength, totalRemaining);
        }

        public override long Position
        {
            get => base.Position - seekStart;
            set => throw new NotSupportedException();
        }

        public PauseStream(Stream innerStream, double pauseAfter)
            : base(innerStream)
        {
            maxLength = (int)Math.Floor(innerStream.Length * pauseAfter) + 1;

            totalRemaining = innerStream.Length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var position = seekStart = base.Seek(offset, origin);

            totalRemaining = base.Length - position;

            return position;
        }

        public void Reset()
        {
            totalRead = 0;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            var remaining = Length - totalRead;

            if (remaining <= 0)
            {
                return 0;
            }

            if (remaining < buffer.Length)
            {
                buffer = buffer[.. (int)remaining];
            }

            var bytesRead = await base.ReadAsync(buffer, cancellationToken);

            totalRead += bytesRead;

            return bytesRead;
        }
    }
}
