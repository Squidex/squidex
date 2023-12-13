// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;

namespace TestSuite.Fixtures;

public class CreatedTeamFixture : ClientFixture
{
    public string TeamName { get; } = $"my-team-{Guid.NewGuid()}";

    public string TeamId => Team.Id;

    public TeamDto Team { get; private set; }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await Factories.CreateAsync(TeamName, async () =>
        {
            try
            {
                var request = new CreateTeamDto
                {
                    Name = TeamName
                };

                Team = await Client.Teams.PostTeamAsync(request);
            }
            catch (SquidexException ex)
            {
                if (ex.StatusCode != 400)
                {
                    throw;
                }
            }

            return true;
        });
    }
}
