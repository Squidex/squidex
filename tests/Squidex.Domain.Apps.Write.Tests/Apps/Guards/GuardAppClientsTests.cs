// ==========================================================================
//  GuardAppClientsTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Apps.Guards
{
    public class GuardAppClientsTests
    {
        private readonly AppClients clients = new AppClients();

        [Fact]
        public void CanAttach_should_throw_execption_if_client_id_is_null()
        {
            var command = new AttachClient();

            Assert.Throws<ValidationException>(() => GuardAppClients.CanAttach(clients, command));
        }

        [Fact]
        public void CanAttach_should_throw_exception_if_client_already_exists()
        {
            var command = new AttachClient { Id = "android" };

            clients.Add("android", "secret");

            Assert.Throws<ValidationException>(() => GuardAppClients.CanAttach(clients, command));
        }

        [Fact]
        public void CanAttach_should_not_throw_exception_if_client_is_free()
        {
            var command = new AttachClient { Id = "ios" };

            clients.Add("android", "secret");

            GuardAppClients.CanAttach(clients, command);
        }

        [Fact]
        public void CanRevoke_should_throw_execption_if_client_id_is_null()
        {
            var command = new RevokeClient();

            Assert.Throws<ValidationException>(() => GuardAppClients.CanRevoke(clients, command));
        }

        [Fact]
        public void CanRevoke_should_throw_exception_if_client_is_not_found()
        {
            var command = new RevokeClient { Id = "ios" };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppClients.CanRevoke(clients, command));
        }

        [Fact]
        public void CanRevoke_should_not_throw_exception_if_client_is_found()
        {
            var command = new RevokeClient { Id = "ios" };

            clients.Add("ios", "secret");

            GuardAppClients.CanRevoke(clients, command);
        }

        [Fact]
        public void CanUpdate_should_throw_execption_if_client_id_is_null()
        {
            var command = new UpdateClient();

            Assert.Throws<ValidationException>(() => GuardAppClients.CanUpdate(clients, command));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_is_not_found()
        {
            var command = new UpdateClient { Id = "ios", Name = "iOS" };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppClients.CanUpdate(clients, command));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_has_no_name_and_permission()
        {
            var command = new UpdateClient { Id = "ios" };

            clients.Add("ios", "secret");

            Assert.Throws<ValidationException>(() => GuardAppClients.CanUpdate(clients, command));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_has_invalid_permission()
        {
            var command = new UpdateClient { Id = "ios", Permission = (AppClientPermission)10 };

            clients.Add("ios", "secret");

            Assert.Throws<ValidationException>(() => GuardAppClients.CanUpdate(clients, command));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_has_same_name()
        {
            var command = new UpdateClient { Id = "ios", Name = "ios" };

            clients.Add("ios", "secret");

            Assert.Throws<ValidationException>(() => GuardAppClients.CanUpdate(clients, command));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_has_same_permission()
        {
            var command = new UpdateClient { Id = "ios", Permission = AppClientPermission.Editor };

            clients.Add("ios", "secret");

            Assert.Throws<ValidationException>(() => GuardAppClients.CanUpdate(clients, command));
        }

        [Fact]
        public void UpdateClient_should_not_throw_exception_if_command_is_valid()
        {
            var command = new UpdateClient { Id = "ios", Name = "iOS", Permission = AppClientPermission.Reader };

            clients.Add("ios", "secret");

            GuardAppClients.CanUpdate(clients, command);
        }
    }
}
