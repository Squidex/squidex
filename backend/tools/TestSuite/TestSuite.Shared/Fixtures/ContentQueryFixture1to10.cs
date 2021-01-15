// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using TestSuite.Model;

namespace TestSuite.Fixtures
{
    public class ContentQueryFixture1to10 : ContentFixture
    {
        public ContentQueryFixture1to10()
            : this("my-reads")
        {
        }

        protected ContentQueryFixture1to10(string schemaName = "my-schema")
            : base(schemaName)
        {
            Task.Run(async () =>
            {
                Dispose();

                for (var i = 10; i > 0; i--)
                {
                    var data = new TestEntityData { Number = i, String = i.ToString() };

                    if (i % 2 == 0)
                    {
                        data.Geo = new { type = "Point", coordinates = new[] { i, i } };
                    }
                    else
                    {
                        data.Geo = new { longitude = i, latitude = i };
                    }

                    await Contents.CreateAsync(data, true);
                }
            }).Wait();
        }

        public override void Dispose()
        {
            Task.Run(async () =>
            {
                var contents = await Contents.GetAsync();

                foreach (var content in contents.Items)
                {
                    await Contents.DeleteAsync(content);
                }
            }).Wait();

            GC.SuppressFinalize(this);
        }
    }
}
