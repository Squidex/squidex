// ==========================================================================
//  FolderAssetStoreTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
            Assert.Throws<ConfigurationException>(() => new FolderAssetStore(CreateInvalidPath(), A.Dummy<ISemanticLog>()).Connect());
        }

        [Fact]
        public void Should_create_directory_when_connecting()
        {
            Sut.Connect();

            Assert.True(Directory.Exists(testFolder));
        }

        [Fact]
        public void Should_calculate_source_url()
        {
            Sut.Connect();

            var id = Guid.NewGuid().ToString();

            Assert.Equal(Path.Combine(testFolder, $"{id}_1"), Sut.GenerateSourceUrl(id, 1, null));
        }

        private static string CreateInvalidPath()
        {
            var windir = Environment.GetEnvironmentVariable("windir");

            return !string.IsNullOrWhiteSpace(windir) ? "Z://invalid" : "/proc/invalid";
        }
    }
}
