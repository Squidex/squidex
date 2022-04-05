// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppClientJsonTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var clients = AppClients.Empty;

            clients = clients.Add("1", "my-secret");
            clients = clients.Add("2", "my-secret");
            clients = clients.Add("3", "my-secret");
            clients = clients.Add("4", "my-secret");

            clients = clients.Update("3", role: Role.Editor);

            clients = clients.Update("3", name: "My Client 3");
            clients = clients.Update("2", name: "My Client 2");

            clients = clients.Update("1", allowAnonymous: true, apiCallsLimit: 3);

            clients = clients.Revoke("4");

            var serialized = clients.SerializeAndDeserialize();

            Assert.Equal(clients, serialized);
        }
    }
}
