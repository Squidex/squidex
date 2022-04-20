// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using TestSuite.Model;

namespace TestSuite.Fixtures
{
    public class ContentQueryFixture1to10 : ContentFixture
    {
        public ContentQueryFixture1to10()
            : base("my-reads")
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await DisposeAsync();

            for (var i = 10; i > 0; i--)
            {
                var text = i.ToString(CultureInfo.InvariantCulture);

                var data = new TestEntityData
                {
                    String = text,
                    Json = JObject.FromObject(new
                    {
                        nested1 = new
                        {
                            nested2 = i
                        }
                    }),
                    Number = i,
                };

                if (i % 2 == 0)
                {
                    data.Geo = new { type = "Point", coordinates = new[] { i, i } };
                }
                else
                {
                    data.Geo = new { longitude = i, latitude = i };
                }

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
}
