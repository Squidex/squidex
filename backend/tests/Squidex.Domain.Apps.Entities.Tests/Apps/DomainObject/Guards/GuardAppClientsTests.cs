// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards
{
    public class GuardAppClientsTests : IClassFixture<TranslationsFixture>
    {
        private readonly AppClients clients_0 = AppClients.Empty;
        private readonly Roles roles = Roles.Empty;

        [Fact]
        public void CanAttach_should_throw_execption_if_client_id_is_null()
        {
            var command = new AttachClient();

            ValidationAssert.Throws(() => GuardAppClients.CanAttach(command, App(clients_0)),
                new ValidationError("Client ID is required.", "Id"));
        }

        [Fact]
        public void CanAttach_should_throw_exception_if_client_already_exists()
        {
            var command = new AttachClient { Id = "android" };

            var clients_1 = clients_0.Add("android", "secret");

            ValidationAssert.Throws(() => GuardAppClients.CanAttach(command, App(clients_1)),
                new ValidationError("A client with the same id already exists."));
        }

        [Fact]
        public void CanAttach_should_not_throw_exception_if_client_is_free()
        {
            var command = new AttachClient { Id = "ios" };

            var clients_1 = clients_0.Add("android", "secret");

            GuardAppClients.CanAttach(command, App(clients_1));
        }

        [Fact]
        public void CanRevoke_should_throw_execption_if_client_id_is_null()
        {
            var command = new RevokeClient();

            ValidationAssert.Throws(() => GuardAppClients.CanRevoke(command, App(clients_0)),
                new ValidationError("Client ID is required.", "Id"));
        }

        [Fact]
        public void CanRevoke_should_throw_exception_if_client_is_not_found()
        {
            var command = new RevokeClient { Id = "ios" };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppClients.CanRevoke(command, App(clients_0)));
        }

        [Fact]
        public void CanRevoke_should_not_throw_exception_if_client_is_found()
        {
            var command = new RevokeClient { Id = "ios" };

            var clients_1 = clients_0.Add("ios", "secret");

            GuardAppClients.CanRevoke(command, App(clients_1));
        }

        [Fact]
        public void CanUpdate_should_throw_execption_if_client_id_is_null()
        {
            var command = new UpdateClient { Name = "iOS" };

            ValidationAssert.Throws(() => GuardAppClients.CanUpdate(command, App(clients_0)),
                new ValidationError("Client ID is required.", "Id"));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_is_not_found()
        {
            var command = new UpdateClient { Id = "ios", Name = "iOS" };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppClients.CanUpdate(command, App(clients_0)));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_has_invalid_role()
        {
            var command = new UpdateClient { Id = "ios", Role = "Invalid" };

            var clients_1 = clients_0.Add("ios", "secret");

            ValidationAssert.Throws(() => GuardAppClients.CanUpdate(command, App(clients_1)),
                new ValidationError("Role is not a valid value.", "Role"));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_api_calls_limit_is_less_than_zero()
        {
            var command = new UpdateClient { Id = "ios", ApiCallsLimit = -10 };

            var clients_1 = clients_0.Add("ios", "secret");

            ValidationAssert.Throws(() => GuardAppClients.CanUpdate(command, App(clients_1)),
                new ValidationError("ApiCallsLimit must be greater or equal to 0.", "ApiCallsLimit"));
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_api_traffic_limit_is_less_than_zero()
        {
            var command = new UpdateClient { Id = "ios", ApiTrafficLimit = -10 };

            var clients_1 = clients_0.Add("ios", "secret");

            ValidationAssert.Throws(() => GuardAppClients.CanUpdate(command, App(clients_1)),
                new ValidationError("ApiTrafficLimit must be greater or equal to 0.", "ApiTrafficLimit"));
        }

        [Fact]
        public void UpdateClient_should_not_throw_exception_if_client_has_same_name()
        {
            var command = new UpdateClient { Id = "ios", Name = "ios" };

            var clients_1 = clients_0.Add("ios", "secret");

            GuardAppClients.CanUpdate(command, App(clients_1));
        }

        [Fact]
        public void UpdateClient_not_should_throw_exception_if_client_has_same_role()
        {
            var command = new UpdateClient { Id = "ios", Role = Role.Editor };

            var clients_1 = clients_0.Add("ios", "secret");

            GuardAppClients.CanUpdate(command, App(clients_1));
        }

        [Fact]
        public void UpdateClient_should_not_throw_exception_if_command_is_valid()
        {
            var command = new UpdateClient { Id = "ios", Name = "iOS", Role = Role.Reader };

            var clients_1 = clients_0.Add("ios", "secret");

            GuardAppClients.CanUpdate(command, App(clients_1));
        }

        private IAppEntity App(AppClients clients)
        {
            var app = A.Fake<IAppEntity>();

            A.CallTo(() => app.Clients)
                .Returns(clients);
            A.CallTo(() => app.Roles)
                .Returns(roles);

            return app;
        }
    }
}
