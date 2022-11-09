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

        await Factories.CreateAsync(AppName, async () =>
        {
            try
            {
                await Apps.PostAppAsync(new CreateAppDto
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

            try
            {
                await Apps.PostLanguageAsync(AppName, new AddLanguageDto
                {
                    Language = "custom"
                });
            }
            catch (SquidexManagementException ex)
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
