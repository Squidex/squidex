// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;

namespace TestSuite.Fixtures;

public class CreatedAppFixture : ClientFixture
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        async Task CreateLanguageAsync(string name)
        {
            try
            {
                var createRequest = new AddLanguageDto
                {
                    Language = name
                };

                await Client.Apps.PostLanguageAsync(createRequest);
            }
            catch (SquidexManagementException ex)
            {
                if (ex.StatusCode != 400)
                {
                    throw;
                }
            }
        }

        await Factories.CreateAsync(AppName, async () =>
        {
            try
            {
                await Client.Apps.PostAppAsync(new CreateAppDto
                {
                    Name = AppName
                });
            }
            catch (SquidexManagementException ex)
            {
                if (ex.StatusCode != 400)
                {
                    throw;
                }
            }

            await CreateLanguageAsync("de");
            await CreateLanguageAsync("custom");

            return true;
        });
    }
}
