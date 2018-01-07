// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public class GuardAppClientsTests
    {
        private readonly AppClients clients_0 = AppClients.Empty;

        [Fact]
        public void CanAttach_should_throw_execption_if_client_id_is_null()
        {
            var command = new AttachClient();

            Assert.Throws<ValidationException>(() => GuardAppClients.CanAttach(clients_0, command));
        }

        [Fact]
        public void CanAttach_should_throw_exception_if_client_already_exists()
        {
            var command = new AttachClient { Id = "android" };

            var clients_1 = clients_0.Add("android", "secret");

            Assert.Throws<ValidationException>(() => GuardAppClients.CanAttach(clients_1, command));
        }

        [Fact]
        public void CanAttach_should_not_throw_exception_if_client_is_free()
        {
            var command = new AttachClient { Id = "ios" };

            var clients_1 = clients_0.Add("android", "secret");

            GuardAppClients.CanAttach(clients_1, command);
        }

        [Fact]
        public void CanRevoke_should_throw_execption_if_client_id_is_null()
        {
            var command = new RevokeClient();

            Assert.Throws<ValidationException>(() => GuardAppClients.CanRevoke(clients_0, command));
        }

        [Fact]
        public void CanRevoke_should_throw_exception_if_client_is_not_found()
        {
            var command = new RevokeClient { Id = "ios" };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppClients.CanRevoke(clients_0, command));
        }

        [Fact]
        public void CanRevoke_should_not_throw_exception_if_client_is_found()
        {
            var command = new RevokeClient { Id = "ios" };

            var clients_1 = clients_0.Add("ios", "secret");

            GuardAppClients.CanRevoke(clients_1, command);
        }

        [Fact]
        public void CanUpdate_should_throw_execption_if_client_id_is_null()
        {
            var command = new UpdateClient();

            Assert.Throws<ValidationException>(() => GuardAppClients.CanUpdate(clients_0, command));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_is_not_found()
        {
            var command = new UpdateClient { Id = "ios", Name = "iOS" };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppClients.CanUpdate(clients_0, command));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_has_no_name_and_permission()
        {
            var command = new UpdateClient { Id = "ios" };

            var clients_1 = clients_0.Add("ios", "secret");

            Assert.Throws<ValidationException>(() => GuardAppClients.CanUpdate(clients_1, command));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_has_invalid_permission()
        {
            var command = new UpdateClient { Id = "ios", Permission = (AppClientPermission)10 };

            var clients_1 = clients_0.Add("ios", "secret");

            Assert.Throws<ValidationException>(() => GuardAppClients.CanUpdate(clients_1, command));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_has_same_name()
        {
            var command = new UpdateClient { Id = "ios", Name = "ios" };

            var clients_1 = clients_0.Add("ios", "secret");

            Assert.Throws<ValidationException>(() => GuardAppClients.CanUpdate(clients_1, command));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_has_same_permission()
        {
            var command = new UpdateClient { Id = "ios", Permission = AppClientPermission.Editor };

            var clients_1 = clients_0.Add("ios", "secret");

            Assert.Throws<ValidationException>(() => GuardAppClients.CanUpdate(clients_1, command));
        }

        [Fact]
        public void UpdateClient_should_not_throw_exception_if_command_is_valid()
        {
            var command = new UpdateClient { Id = "ios", Name = "iOS", Permission = AppClientPermission.Reader };

            var clients_1 = clients_0.Add("ios", "secret");

            GuardAppClients.CanUpdate(clients_1, command);
        }
    }
}
