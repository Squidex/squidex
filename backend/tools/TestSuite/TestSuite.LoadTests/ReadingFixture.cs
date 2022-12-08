// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Model;

namespace TestSuite.LoadTests;

public sealed class ReadingFixture : TestSchemaFixtureBase
{
    public ReadingFixture()
        : base("benchmark-reading")
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await DisposeAsync();

        var current = await Contents.GetAsync(new ContentQuery
        {
            Top = 0
        });

        var countTotal = (int)current.Total;
        var countMissing = 100 - countTotal;

        for (var index = countMissing; index > 0; index--)
        {
            var data = new TestEntityData
            {
                Number = index,
                Json = JObject.FromObject(new
                {
                    nested0 = index,
                    nested1 = new
                    {
                        nested2 = index
                    }
                }),
                String = index.ToString(CultureInfo.InvariantCulture)
            };

            await Contents.CreateAsync(data, ContentCreateOptions.AsPublish);
        }
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();

        var contents = await Contents.GetAsync();

        foreach (var content in contents.Items)
        {
            await Contents.DeleteAsync(content);
        }
    }
}
