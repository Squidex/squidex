// ==========================================================================
//  AppContributorsTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppContributorsTests
    {
        private readonly JsonSerializer serializer = TestData.DefaultSerializer();
        private readonly AppContributors sut = new AppContributors();

        [Fact]
        public void Should_assign_new_contributor()
        {
            sut.Assign("1", AppContributorPermission.Developer);
            sut.Assign("2", AppContributorPermission.Editor);

            Assert.Equal(AppContributorPermission.Developer, sut["1"]);
            Assert.Equal(AppContributorPermission.Editor, sut["2"]);
        }

        [Fact]
        public void Should_replace_contributor_if_already_exists()
        {
            sut.Assign("1", AppContributorPermission.Developer);
            sut.Assign("1", AppContributorPermission.Owner);

            Assert.Equal(AppContributorPermission.Owner, sut["1"]);
        }

        [Fact]
        public void Should_remove_contributor()
        {
            sut.Assign("1", AppContributorPermission.Developer);
            sut.Remove("1");

            Assert.Empty(sut);
        }

        [Fact]
        public void Should_do_nothing_if_contributor_to_remove_not_found()
        {
            sut.Remove("2");

            Assert.Empty(sut);
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            sut.Assign("1", AppContributorPermission.Developer);
            sut.Assign("2", AppContributorPermission.Editor);
            sut.Assign("3", AppContributorPermission.Owner);

            var serialized = JToken.FromObject(sut, serializer).ToObject<AppContributors>(serializer);

            serialized.ShouldBeEquivalentTo(sut);
        }
    }
}
