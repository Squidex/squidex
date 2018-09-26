// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppClientsTests
    {
        private readonly AppClients clients_0 = AppClients.Empty.Add("1", "my-secret");

        [Fact]
        public void Should_assign_client()
        {
            var clients_1 = clients_0.Add("2", "my-secret");

            clients_1["2"].Should().BeEquivalentTo(new AppClient("2", "my-secret", AppClientPermission.Editor));
        }

        [Fact]
        public void Should_assign_clients_with_permission()
        {
            var clients_1 = clients_0.Add("2", new AppClient("my-name", "my-secret", AppClientPermission.Reader));

            clients_1["2"].Should().BeEquivalentTo(new AppClient("my-name", "my-secret", AppClientPermission.Reader));
        }

        [Fact]
        public void Should_throw_exception_if_assigning_clients_with_same_id()
        {
            var clients_1 = clients_0.Add("2", "my-secret");

            Assert.Throws<ArgumentException>(() => clients_1.Add("2", "my-secret"));
        }

        [Fact]
        public void Should_rename_client()
        {
            var clients_1 = clients_0.Rename("1", "new-name");

            clients_1["1"].Should().BeEquivalentTo(new AppClient("new-name", "my-secret", AppClientPermission.Editor));
        }

        [Fact]
        public void Should_return_same_clients_if_client_to_rename_not_found()
        {
            var clients_1 = clients_0.Rename("2", "new-name");

            Assert.Same(clients_0, clients_1);
        }

        [Fact]
        public void Should_update_client()
        {
            var client_1 = clients_0.Update("1", AppClientPermission.Reader);

            client_1["1"].Should().BeEquivalentTo(new AppClient("1", "my-secret", AppClientPermission.Reader));
        }

        [Fact]
        public void Should_return_same_clients_if_client_to_update_not_found()
        {
            var clients_1 = clients_0.Update("2", AppClientPermission.Reader);

            Assert.Same(clients_0, clients_1);
        }

        [Fact]
        public void Should_revoke_client()
        {
            var clients_1 = clients_0.Revoke("1");

            Assert.Empty(clients_1);
        }

        [Fact]
        public void Should_do_nothing_if_client_to_revoke_not_found()
        {
            var clients_1 = clients_0.Revoke("2");

            Assert.NotSame(clients_0, clients_1);
        }
    }
}
