// ==========================================================================
//  AssetFieldTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Core.Schemas
{
    public class AssetsFieldTests
    {
        private readonly Mock<IAssetTester> assetTester = new Mock<IAssetTester>();
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            Assert.Equal("my-asset", sut.Name);
        }

        [Fact]
        public void Should_clone_object()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            Assert.NotEqual(sut, sut.Enable());
        }

        [Fact]
        public async Task Should_not_add_error_if_assets_are_valid()
        {
            var assetId = Guid.NewGuid();

            assetTester.Setup(x => x.IsValidAsync(assetId)).Returns(TaskHelper.True);

            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            await sut.ValidateAsync(CreateValue(assetId), false, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_assets_are_null_and_valid()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            await sut.ValidateAsync(CreateValue(null), false, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_assets_are_required_and_null()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, new AssetsFieldProperties { IsRequired = true }, assetTester.Object);

            await sut.ValidateAsync(CreateValue(null), false, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required" });
        }

        [Fact]
        public async Task Should_add_errors_if_assets_are_required_and_empty()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, new AssetsFieldProperties { IsRequired = true }, assetTester.Object);

            await sut.ValidateAsync(CreateValue(), false, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required" });
        }

        [Fact]
        public async Task Should_add_errors_if_value_is_not_valid()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            await sut.ValidateAsync("invalid", false, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not a valid value" });
        }

        [Fact]
        public async Task Should_add_errors_if_asset_are_not_valid()
        {
            var assetId = Guid.NewGuid();

            assetTester.Setup(x => x.IsValidAsync(assetId)).Returns(TaskHelper.False);

            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            await sut.ValidateAsync(CreateValue(assetId), false, errors);

            errors.ShouldBeEquivalentTo(
                new[] { $"<FIELD> contains invalid asset '{assetId}'" });
        }

        private static JToken CreateValue(params Guid[] ids)
        {
            return ids == null ? JValue.CreateNull() : (JToken)new JArray(ids);
        }
    }
}
