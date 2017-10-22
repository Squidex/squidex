// ==========================================================================
//  AppClientsTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppClientsTests
    {
        private readonly JsonSerializer serializer = TestData.DefaultSerializer();
        private readonly AppClients sut = new AppClients();

        public AppClientsTests()
        {
            sut.Add("1", "my-secret");
        }

        [Fact]
        public void Should_assign_client()
        {
            sut.Add("2", "my-secret");

            sut["2"].ShouldBeEquivalentTo(new AppClient("2", "my-secret", AppClientPermission.Editor));
        }

        [Fact]
        public void Should_assign_client_with_permission()
        {
            sut.Add("2", new AppClient("my-name", "my-secret", AppClientPermission.Reader));

            sut["2"].ShouldBeEquivalentTo(new AppClient("my-name", "my-secret", AppClientPermission.Reader));
        }

        [Fact]
        public void Should_throw_exception_if_assigning_client_with_same_id()
        {
            sut.Add("2", "my-secret");

            Assert.Throws<ArgumentException>(() => sut.Add("2", "my-secret"));
        }

        [Fact]
        public void Should_rename_client()
        {
            sut["1"].Rename("my-name");

            sut["1"].ShouldBeEquivalentTo(new AppClient("my-name", "my-secret", AppClientPermission.Editor));
        }

        [Fact]
        public void Should_update_client()
        {
            sut["1"].Update(AppClientPermission.Reader);

            sut["1"].ShouldBeEquivalentTo(new AppClient("1", "my-secret", AppClientPermission.Reader));
        }

        [Fact]
        public void Should_revoke_client()
        {
            sut.Revoke("1");

            Assert.Empty(sut);
        }

        [Fact]
        public void Should_do_nothing_if_client_to_revoke_not_found()
        {
            sut.Revoke("2");

            Assert.Single(sut);
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            sut.Add("2", "my-secret");
            sut.Add("3", "my-secret");
            sut.Add("4", "my-secret");

            sut["3"].Update(AppClientPermission.Editor);

            sut["3"].Rename("My Client 3");
            sut["2"].Rename("My Client 2");

            sut.Revoke("4");

            var appClients = JToken.FromObject(sut, serializer).ToObject<AppClients>(serializer);

            appClients.ShouldBeEquivalentTo(sut);
        }
    }
}
