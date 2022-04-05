// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

            Assert.Equal(new AppClient("2", "my-secret"), clients_1["2"]);
        }

        [Fact]
        public void Should_assign_clients_with_permission()
        {
            var clients_1 = clients_0.Add("2", "my-secret", Role.Reader);

            Assert.Equal(new AppClient("2", "my-secret") with { Role = Role.Reader }, clients_1["2"]);
        }

        [Fact]
        public void Should_do_nothing_if_assigning_client_with_same_id()
        {
            var clients_1 = clients_0.Add("1", "my-secret");

            Assert.Same(clients_0, clients_1);
        }

        [Fact]
        public void Should_update_client_with_role()
        {
            var client_1 = clients_0.Update("1", role: Role.Reader);

            Assert.Equal(new AppClient("1", "my-secret") with { Role = Role.Reader }, client_1["1"]);
        }

        [Fact]
        public void Should_update_client_with_name()
        {
            var client_1 = clients_0.Update("1", name: "New-Name");

            Assert.Equal(new AppClient("New-Name", "my-secret"), client_1["1"]);
        }

        [Fact]
        public void Should_update_client_with_allow_anonymous()
        {
            var client_1 = clients_0.Update("1", allowAnonymous: true);

            Assert.Equal(new AppClient("1", "my-secret") with { AllowAnonymous = true }, client_1["1"]);
        }

        [Fact]
        public void Should_update_client_with_allow_api_calls_limit()
        {
            var client_1 = clients_0.Update("1", apiCallsLimit: 1000);

            Assert.Equal(new AppClient("1", "my-secret") with { ApiCallsLimit = 1000 }, client_1["1"]);
        }

        [Fact]
        public void Should_update_client_with_allow_api_traffic_limit()
        {
            var client_1 = clients_0.Update("1", apiTrafficLimit: 1000);

            Assert.Equal(new AppClient("1", "my-secret") with { ApiTrafficLimit = 1000 }, client_1["1"]);
        }

        [Fact]
        public void Should_return_same_clients_if_client_to_update_not_found()
        {
            var clients_1 = clients_0.Update("2", role: Role.Reader);

            Assert.Same(clients_0, clients_1);
        }

        [Fact]
        public void Should_revoke_client()
        {
            var clients_1 = clients_0.Add("2", "secret2");
            var clients_2 = clients_1.Add("3", "secret3");
            var clients_3 = clients_2.Revoke("2");

            Assert.Equal(new[] { "1", "3" }, clients_3.Keys);
        }

        [Fact]
        public void Should_do_nothing_if_client_to_revoke_not_found()
        {
            var clients_1 = clients_0.Revoke("2");

            Assert.Same(clients_0, clients_1);
        }
    }
}
