// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using FakeItEasy;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Infrastructure.Assets
{
    public class FolderAssetStoreTests : AssetStoreTests<FolderAssetStore>
    {
        private readonly string testFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        public override FolderAssetStore CreateStore()
        {
            return new FolderAssetStore(testFolder, A.Dummy<ISemanticLog>());
        }

        public override void Dispose()
        {
            if (Directory.Exists(testFolder))
            {
                Directory.Delete(testFolder, true);
            }
        }

        [Fact]
        public void Should_throw_when_creating_directory_failed()
        {
            Assert.Throws<ConfigurationException>(() => new FolderAssetStore(CreateInvalidPath(), A.Dummy<ISemanticLog>()).InitializeAsync().Wait());
        }

        [Fact]
        public void Should_create_directory_when_connecting()
        {
            Assert.True(Directory.Exists(testFolder));
        }

        [Fact]
        public void Should_calculate_source_url()
        {
            var url = Sut.GenerateSourceUrl(AssetId, 1, null);

            Assert.Equal(Path.Combine(testFolder, $"{AssetId}_1"), url);
        }

        private static string CreateInvalidPath()
        {
            var windir = Environment.GetEnvironmentVariable("windir");

            return !string.IsNullOrWhiteSpace(windir) ? "Z://invalid" : "/proc/invalid";
        }
    }
}
