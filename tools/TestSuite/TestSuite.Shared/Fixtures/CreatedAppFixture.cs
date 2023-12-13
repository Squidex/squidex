// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;

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
            catch (SquidexException ex)
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
                var createRequest = new CreateAppDto
                {
                    Name = AppName
                };

                await Client.Apps.PostAppAsync(createRequest);
            }
            catch (SquidexException ex)
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
