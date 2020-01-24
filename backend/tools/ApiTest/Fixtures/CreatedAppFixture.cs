// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using ApiTest.Model;
using Squidex.ClientLibrary.Management;

namespace ApiTest.Fixtures
{
    public class CreatedAppFixture : ClientFixture
    {
        public string AppName { get; } = TestClient.TestAppName;

        public CreatedAppFixture()
        {
            Task.Run(async () =>
            {
                try
                {
                    await Apps.PostAppAsync(new CreateAppDto { Name = AppName });
                }
                catch (SquidexManagementException ex)
                {
                    if (ex.StatusCode != 400)
                    {
                        throw;
                    }
                }

                await Apps.PostContributorAsync(AppName, new AssignContributorDto { ContributorId = "sebastian@squidex.io", Invite = true, Role = "Owner" });
            }).Wait();
        }
    }
}
