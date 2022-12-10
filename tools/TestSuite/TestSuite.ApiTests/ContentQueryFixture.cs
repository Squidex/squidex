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

namespace TestSuite.ApiTests;

public sealed class ContentQueryFixture : TestSchemaFixtureBase
{
    public ContentQueryFixture()
        : base("my-reads")
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await DisposeAsync();

        for (var index = 10; index > 0; index--)
        {
            var data = new TestEntityData
            {
                Number = index,
                Json = JObject.FromObject(new
                {
                    nested1 = new
                    {
                        nested2 = index
                    }
                }),
                Geo = GeoJson.Point(
                    index + 100,
                    index,
                    oldFormat: index % 2 == 1),
                Localized = new Dictionary<string, string>
                {
                    ["en"] = index.ToString(CultureInfo.InvariantCulture)
                },
                String = index.ToString(CultureInfo.InvariantCulture),
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
