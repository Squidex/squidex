// ==========================================================================
//  AssetFieldTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void Should_return_ids()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            var result = sut.GetReferencedIds(CreateValue(id1, id2)).ToArray();

            Assert.Equal(new[] { id1, id2 }, result);
        }

        [Fact]
        public void Should_empty_list_for_referenced_ids_when_null()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            var result = sut.GetReferencedIds(null).ToArray();

            Assert.Empty(result);
        }

        [Fact]
        public void Should_empty_list_for_referenced_ids_when_other_type()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            var result = sut.GetReferencedIds("invalid").ToArray();

            Assert.Empty(result);
        }

        [Fact]
        public void Should_return_null_when_removing_references_from_null_array()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            var result = sut.RemoveDeletedReferences(null, null);

            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_when_removing_references_from_null_json_array()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            var result = sut.RemoveDeletedReferences(JValue.CreateNull(), null);

            Assert.Null(result);
        }

        [Fact]
        public void Should_remove_deleted_references()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            var result = sut.RemoveDeletedReferences(CreateValue(id1, id2), new HashSet<Guid>(new[] { id2 }));

            Assert.Equal(CreateValue(id1), result);
        }

        [Fact]
        public void Should_return_same_token_when_removing_references_and_nothing_to_remove()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, assetTester.Object);

            var token = CreateValue(id1, id2);
            var result = sut.RemoveDeletedReferences(token, new HashSet<Guid>(new[] { Guid.NewGuid() }));

            Assert.Same(token, result);
        }

        private static JToken CreateValue(params Guid[] ids)
        {
            return ids == null ? JValue.CreateNull() : (JToken)new JArray(ids);
        }
    }
}
