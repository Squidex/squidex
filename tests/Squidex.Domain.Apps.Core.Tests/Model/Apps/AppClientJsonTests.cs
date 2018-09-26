// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppClientJsonTests
    {
        private readonly JsonSerializer serializer = TestData.DefaultSerializer();

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var clients = AppClients.Empty;

            clients = clients.Add("1", "my-secret");
            clients = clients.Add("2", "my-secret");
            clients = clients.Add("3", "my-secret");
            clients = clients.Add("4", "my-secret");

            clients = clients.Update("3", AppClientPermission.Editor);

            clients = clients.Rename("3", "My Client 3");
            clients = clients.Rename("2", "My Client 2");

            clients = clients.Revoke("4");

            var appClients = JToken.FromObject(clients, serializer).ToObject<AppClients>(serializer);

            appClients.Should().BeEquivalentTo(clients);
        }
    }
}
