// ==========================================================================
//  AssetsFieldTests.cs
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
using Newtonsoft.Json.Linq;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class AssetsFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            Assert.Equal("my-asset", sut.Name);
        }

        [Fact]
        public void Should_clone_object()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            Assert.NotEqual(sut, sut.Enable());
        }

        [Fact]
        public async Task Should_not_add_error_if_assets_are_valid()
        {
            var assetId = Guid.NewGuid();

            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            await sut.ValidateAsync(CreateValue(assetId), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_assets_are_null_and_valid()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            await sut.ValidateAsync(CreateValue(null), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_assets_are_required_and_null()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, new AssetsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required" });
        }

        [Fact]
        public async Task Should_add_errors_if_assets_are_required_and_empty()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, new AssetsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required" });
        }

        [Fact]
        public async Task Should_add_errors_if_value_is_not_valid()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            await sut.ValidateAsync("invalid", errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not a valid value" });
        }

        [Fact]
        public async Task Should_add_errors_if_value_has_not_enough_items()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, new AssetsFieldProperties { MinItems = 3 });

            await sut.ValidateAsync(CreateValue(Guid.NewGuid(), Guid.NewGuid()), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must have at least 3 asset(s)" });
        }

        [Fact]
        public async Task Should_add_errors_if_value_has_too_much_items()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant, new AssetsFieldProperties { MaxItems = 1 });

            await sut.ValidateAsync(CreateValue(Guid.NewGuid(), Guid.NewGuid()), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must have not more than 1 asset(s)" });
        }

        [Fact]
        public async Task Should_add_errors_if_asset_are_not_valid()
        {
            var assetId = Guid.NewGuid();

            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            await sut.ValidateAsync(CreateValue(assetId), errors, ValidationTestExtensions.InvalidContext(assetId));

            errors.ShouldBeEquivalentTo(
                new[] { $"<FIELD> contains invalid asset '{assetId}'" });
        }

        [Fact]
        public void Should_return_ids()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            var result = sut.GetReferencedIds(CreateValue(id1, id2)).ToArray();

            Assert.Equal(new[] { id1, id2 }, result);
        }

        [Fact]
        public void Should_empty_list_for_referenced_ids_when_null()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            var result = sut.GetReferencedIds(null).ToArray();

            Assert.Empty(result);
        }

        [Fact]
        public void Should_empty_list_for_referenced_ids_when_other_type()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            var result = sut.GetReferencedIds("invalid").ToArray();

            Assert.Empty(result);
        }

        [Fact]
        public void Should_return_null_when_removing_references_from_null_array()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            var result = sut.RemoveDeletedReferences(null, null);

            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_when_removing_references_from_null_json_array()
        {
            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            var result = sut.RemoveDeletedReferences(JValue.CreateNull(), null);

            Assert.Null(result);
        }

        [Fact]
        public void Should_remove_deleted_references()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            var result = sut.RemoveDeletedReferences(CreateValue(id1, id2), new HashSet<Guid>(new[] { id2 }));

            Assert.Equal(CreateValue(id1), result);
        }

        [Fact]
        public void Should_return_same_token_when_removing_references_and_nothing_to_remove()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = new AssetsField(1, "my-asset", Partitioning.Invariant);

            var token = CreateValue(id1, id2);
            var result = sut.RemoveDeletedReferences(token, new HashSet<Guid>(new[] { Guid.NewGuid() }));

            Assert.Same(token, result);
        }

        private static JToken CreateValue(params Guid[] ids)
        {
            return ids == null ? JValue.CreateNull() : (JToken)new JArray(ids.OfType<object>().ToArray());
        }
    }
}
