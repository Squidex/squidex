// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Guards
{
    public class GuardAssetTests
    {
        [Fact]
        public void CanRename_should_throw_exception_if_name_not_defined()
        {
            var command = new RenameAsset();

            Assert.Throws<ValidationException>(() => GuardAsset.CanRename(command, "asset-name"));
        }

        [Fact]
        public void CanRename_should_throw_exception_if_name_are_the_same()
        {
            var command = new RenameAsset { FileName = "asset-name" };

            Assert.Throws<ValidationException>(() => GuardAsset.CanRename(command, "asset-name"));
        }

        [Fact]
        public void CanRename_should_not_throw_exception_if_name_are_different()
        {
            var command = new RenameAsset { FileName = "new-name" };

            GuardAsset.CanRename(command, "asset-name");
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
