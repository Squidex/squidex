// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Guards
{
    public class GuardAssetTests
    {
        [Fact]
        public void CanAnnotate_should_throw_exception_if_nothing_defined()
        {
            var command = new AnnotateAsset();

            ValidationAssert.Throws(() => GuardAsset.CanAnnotate(command, "asset-name", "asset-slug"),
                new ValidationError("Either file name, slug or tags must be defined.", "FileName", "Slug", "Tags"));
        }

        [Fact]
        public void CanAnnotate_should_throw_exception_if_names_are_the_same()
        {
            var command = new AnnotateAsset { FileName = "asset-name" };

            ValidationAssert.Throws(() => GuardAsset.CanAnnotate(command, "asset-name", "asset-slug"),
                new ValidationError("Asset has already this name.", "FileName"));
        }

        [Fact]
        public void CanAnnotate_should_throw_exception_if_slugs_are_the_same()
        {
            var command = new AnnotateAsset { Slug = "asset-slug" };

            ValidationAssert.Throws(() => GuardAsset.CanAnnotate(command, "asset-name", "asset-slug"),
                new ValidationError("Asset has already this slug.", "Slug"));
        }

        [Fact]
        public void CanAnnotate_should_not_throw_exception_if_names_are_different()
        {
            var command = new AnnotateAsset { FileName = "new-name", Slug = "new-slug" };

            GuardAsset.CanAnnotate(command, "asset-name", "asset-slug");
        }

        [Fact]
        public void CanCreate_should_not_throw_exception()
        {
            var command = new CreateAsset();

            GuardAsset.CanCreate(command);
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception()
        {
            var command = new UpdateAsset();

            GuardAsset.CanUpdate(command);
        }

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            var command = new DeleteAsset();

            GuardAsset.CanDelete(command);
        }
    }
}
