// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppContributorsTests
    {
        private readonly AppContributors contributors_0 = AppContributors.Empty;

        [Fact]
        public void Should_assign_new_contributor()
        {
            var contributors_1 = contributors_0.Assign("1", AppContributorPermission.Developer);
            var contributors_2 = contributors_1.Assign("2", AppContributorPermission.Editor);

            Assert.Equal(AppContributorPermission.Developer, contributors_2["1"]);
            Assert.Equal(AppContributorPermission.Editor, contributors_2["2"]);
        }

        [Fact]
        public void Should_replace_contributor_if_already_exists()
        {
            var contributors_1 = contributors_0.Assign("1", AppContributorPermission.Developer);
            var contributors_2 = contributors_1.Assign("1", AppContributorPermission.Owner);

            Assert.Equal(AppContributorPermission.Owner, contributors_2["1"]);
        }

        [Fact]
        public void Should_remove_contributor()
        {
            var contributors_1 = contributors_0.Assign("1", AppContributorPermission.Developer);
            var contributors_2 = contributors_1.Remove("1");

            Assert.Empty(contributors_2);
        }

        [Fact]
        public void Should_do_nothing_if_contributor_to_remove_not_found()
        {
            var contributors_1 = contributors_0.Remove("2");

            Assert.Empty(contributors_1);
        }
    }
}
