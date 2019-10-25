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
    public class FolderAssetStoreTests : AssetStoreTests<FolderAssetStore>, IClassFixture<FolderAssetStoreFixture>
    {
        private readonly FolderAssetStoreFixture fixture;

        public FolderAssetStoreTests(FolderAssetStoreFixture fixture)
        {
            this.fixture = fixture;
        }

        public override FolderAssetStore CreateStore()
        {
            return fixture.AssetStore;
        }

        [Fact]
        public void Should_throw_when_creating_directory_failed()
        {
            Assert.Throws<ConfigurationException>(() => new FolderAssetStore(CreateInvalidPath(), A.Dummy<ISemanticLog>()).InitializeAsync().Wait());
        }

        [Fact]
        public void Should_create_directory_when_connecting()
        {
            Assert.True(Directory.Exists(fixture.TestFolder));
        }

        [Fact]
        public void Should_calculate_source_url()
        {
            var url = Sut.GeneratePublicUrl(FileName);

            Assert.Null(url);
        }

        private static string CreateInvalidPath()
        {
            var windir = Environment.GetEnvironmentVariable("windir");

            return !string.IsNullOrWhiteSpace(windir) ? "Z://invalid" : "/proc/invalid";
        }
    }
}
