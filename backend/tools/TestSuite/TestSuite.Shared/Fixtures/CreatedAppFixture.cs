// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;

namespace TestSuite.Fixtures
{
    public class CreatedAppFixture : ClientFixture
    {
        private static readonly string[] Contributors =
        {
            "hello@squidex.io"
        };

        private static int isCreated;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            if (Interlocked.Increment(ref isCreated) == 0)
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

                var invite = new AssignContributorDto { Invite = true, Role = "Owner" };

                foreach (var contributor in Contributors)
                {
                    invite.ContributorId = contributor;

                    await Apps.PostContributorAsync(AppName, invite);
                }

                try
                {
                    await Apps.PostLanguageAsync(AppName, new AddLanguageDto
                    {
                        Language = "de"
                    });
                }
                catch (SquidexManagementException ex)
                {
                    if (ex.StatusCode != 400)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
