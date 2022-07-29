// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.ClientLibrary.Management;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class AssetTests : IClassFixture<AssetFixture>
    {
        private ProgressHandler progress = new ProgressHandler();

        public AssetFixture _ { get; }

        public AssetTests(AssetFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_upload_asset()
        {
            // STEP 1: Create asset
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");

            await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var downloaded = await _.DownloadAsync(asset_1);

                // Should dowload with correct size.
                Assert.Equal(stream.Length, downloaded.Length);
            }
        }

        [Fact]
        public async Task Should_upload_asset_using_tus()
        {
            // STEP 1: Create asset
            var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

            await using (fileParameter.Data)
            {
                await _.Assets.UploadAssetAsync(_.AppName, fileParameter,
                    progress.AsOptions());
            }

            Assert.NotEmpty(progress.Progress);
            Assert.NotNull(progress.Asset);
            Assert.Null(progress.Exception);

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

                var pausingStream = new PauseStream(fileParameter.Data, 0.5);
                var pausingFile = new FileParameter(pausingStream, fileParameter.FileName, fileParameter.ContentType);

                var numUploads = 0;

                await using (pausingFile.Data)
                {
                    using var cts = new CancellationTokenSource(5000);

                    while (progress.Asset == null && progress.Exception == null)
                    {
                        pausingStream.Reset();

                        await _.Assets.UploadAssetAsync(_.AppName, pausingFile,
                            progress.AsOptions(), cts.Token);

                        await Task.Delay(50, cts.Token);

                        numUploads++;
                    }
                }

                Assert.NotEmpty(progress.Progress);
                Assert.NotNull(progress.Asset);
                Assert.Null(progress.Exception);
                Assert.True(numUploads > 1);

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
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png", id: id);

            Assert.Equal(id, asset_1.Id);
        }

        [Fact]
        public async Task Should_upload_asset_with_custom_id_using_tus()
        {
            var id = Guid.NewGuid().ToString();

            // STEP 1: Create asset
            var fileParameter = FileParameter.FromPath("Assets/logo-squared.png");

            await _.Assets.UploadAssetAsync(_.AppName, fileParameter,
                progress.AsOptions(id));

            Assert.Equal(id, progress.Asset?.Id);
        }

        [Fact]
        public async Task Should_not_create_asset_with_custom_id_twice()
        {
            var id = Guid.NewGuid().ToString();

            // STEP 1: Create asset
            await _.UploadFileAsync("Assets/logo-squared.png", "image/png", id: id);


            // STEP 2: Create a new item with a custom id.
            var ex = await Assert.ThrowsAnyAsync<SquidexManagementException>(() =>
            {
                return _.UploadFileAsync("Assets/logo-squared.png", "image/png", id: id);
            });

            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task Should_not_create_very_big_asset()
        {
            // STEP 1: Create small asset
            await _.UploadFileAsync(1_000_000);


            // STEP 2: Create big asset
            var ex = await Assert.ThrowsAnyAsync<Exception>(() =>
            {
                return _.UploadFileAsync(10_000_000);
            });

            // Client library cannot catch this exception properly.
            Assert.True(ex is HttpRequestException || ex is SquidexManagementException);
        }

        [Fact]
        public async Task Should_replace_asset()
        {
            // STEP 1: Create asset
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


            // STEP 2: Reupload asset
            var asset_2 = await _.UploadFileAsync("Assets/logo-wide.png", asset_1);

            await using (var stream = new FileStream("Assets/logo-wide.png", FileMode.Open))
            {
                var downloaded = await _.DownloadAsync(asset_2);

                // Should dowload with correct size.
                Assert.Equal(stream.Length, downloaded.Length);
            }
        }

        [Fact]
        public async Task Should_replace_asset_using_tus()
        {
            // STEP 1: Create asset
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


            // STEP 2: Reupload asset
            var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

            await using (fileParameter.Data)
            {
                await _.Assets.UploadAssetAsync(_.AppName, fileParameter,
                    progress.AsOptions(asset_1.Id));
            }

            Assert.NotNull(progress.Asset);
            Assert.NotEmpty(progress.Progress);
            Assert.Null(progress.Exception);

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
            for (var i = 0; i < 5; i++)
            {
                // STEP 1: Create asset
                var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


                // STEP 2: Reupload asset
                progress = new ProgressHandler();

                var fileParameter = FileParameter.FromPath("Assets/SampleVideo_1280x720_1mb.mp4");

                var pausingStream = new PauseStream(fileParameter.Data, 0.5);
                var pausingFile = new FileParameter(pausingStream, fileParameter.FileName, fileParameter.ContentType);

                var numUploads = 0;

                await using (pausingFile.Data)
                {
                    using var cts = new CancellationTokenSource(5000);

                    while (progress.Asset == null && progress.Exception == null)
                    {
                        pausingStream.Reset();

                        await _.Assets.UploadAssetAsync(_.AppName, pausingFile,
                            progress.AsOptions(asset_1.Id), cts.Token);

                        await Task.Delay(50, cts.Token);

                        numUploads++;
                    }
                }

                Assert.NotEmpty(progress.Progress);
                Assert.NotNull(progress.Asset);
                Assert.Null(progress.Exception);
                Assert.True(numUploads > 1);

                await using (var stream = new FileStream("Assets/SampleVideo_1280x720_1mb.mp4", FileMode.Open))
                {
                    var downloaded = await _.DownloadAsync(progress.Asset);

                    // Should dowload with correct size.
                    Assert.Equal(stream.Length, downloaded.Length);
                }
            }
        }

        [Fact]
        public async Task Should_annote_asset()
        {
            // STEP 1: Create asset
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


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


            // STEP 3: Annotate slug.
            var slugRequest = new AnnotateAssetDto { Slug = "my-image" };

            var asset_3 = await _.Assets.PutAssetAsync(_.AppName, asset_2.Id, slugRequest);

            // Should provide updated slug.
            Assert.Equal(slugRequest.Slug, asset_3.Slug);


            // STEP 3: Annotate file name.
            var fileNameRequest = new AnnotateAssetDto { FileName = "My Image" };

            var asset_4 = await _.Assets.PutAssetAsync(_.AppName, asset_3.Id, fileNameRequest);

            // Should provide updated file name.
            Assert.Equal(fileNameRequest.FileName, asset_4.FileName);
        }

        [Fact]
        public async Task Should_protect_asset()
        {
            var fileName = $"{Guid.NewGuid()}.png";

            // STEP 1: Create asset
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


            // STEP 2: Download asset
            await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var downloaded = await _.DownloadAsync(asset_1);

                // Should dowload with correct size.
                Assert.Equal(stream.Length, downloaded.Length);
            }


            // STEP 4: Protect asset
            var protectRequest = new AnnotateAssetDto { IsProtected = true };

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
                Assert.Contains("403", ex.Message, StringComparison.Ordinal);
            }


            // STEP 6: Download asset without key and version.
            await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var ex = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
                {
                    return _.DownloadAsync(asset_1, 0);
                });

                // Should return 403 when not authenticated.
                Assert.Contains("403", ex.Message, StringComparison.Ordinal);
            }
        }

        [Fact]
        public async Task Should_query_asset_by_metadata()
        {
            // STEP 1: Create asset
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


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
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


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
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png", parentId: folder.Id);


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
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png", null, folder_2.Id);


            // STEP 4: Create asset outside folder
            var asset_2 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


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
            var asset = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


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
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


            // STEP 2: Delete asset
            await _.Assets.DeleteAssetAsync(_.AppName, asset_1.Id, permanent: permanent);


            // STEP 3: Recreate asset
            var asset_2 = await _.UploadFileAsync("Assets/logo-wide.png", "image/png");

            Assert.NotEqual(asset_1.FileSize, asset_2.FileSize);
        }

        public class ProgressHandler : IAssetProgressHandler
        {
            public string FileId { get; private set; }

            public List<int> Progress { get; } = new List<int>();

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
                if (!@event.Exception.ToString().Contains("PAUSED", StringComparison.OrdinalIgnoreCase))
                {
                    Exception = @event.Exception;
                }

                return Task.CompletedTask;
            }
        }

        public class PauseStream : DelegateStream
        {
            private readonly double pauseAfter = 1;
            private int totalRead;

            public PauseStream(Stream innerStream, double pauseAfter)
                : base(innerStream)
            {
                this.pauseAfter = pauseAfter;
            }

            public void Reset()
            {
                totalRead = 0;
            }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
                CancellationToken cancellationToken = default)
            {
                if (Position >= Length)
                {
                    return 0;
                }

                if (totalRead >= Length * pauseAfter)
                {
                    throw new InvalidOperationException("PAUSED");
                }

                var bytesRead = await base.ReadAsync(buffer, cancellationToken);

                totalRead += bytesRead;

                return bytesRead;
            }
        }
    }
}
