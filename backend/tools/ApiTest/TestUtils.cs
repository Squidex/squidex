// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using ApiTest.Model;
using Squidex.ClientLibrary.Management;
using Xunit;

namespace ApiTest
{
    public sealed class TestUtils
    {
        [Fact]
        public async Task GenerateAppManyContributorsAsync()
        {
            var client = TestClient.ClientManager.CreateAppsClient();

            for (var i = 0; i < 200; i++)
            {
                await client.PostContributorAsync("test", new AssignContributorDto
                {
                    ContributorId = $"hello{i}@squidex.io", Invite = true, Role = "Editor"
                });
            }
        }
    }
}
