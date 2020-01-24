// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using ApiTest.Model;
using Squidex.ClientLibrary;

namespace ApiTest.Fixtures
{
    public class ContentQueryFixture : ContentFixture
    {
        public ContentQueryFixture()
        {
            Task.Run(async () =>
            {
                Dispose();

                for (var i = 10; i > 0; i--)
                {
                    await Contents.CreateAsync(new TestEntityData { Number = i }, true);
                }
            }).Wait();
        }

        public override void Dispose()
        {
            Task.Run(async () =>
            {
                var contents = await Contents.GetAllAsync();

                foreach (var content in contents.Items)
                {
                    await Contents.DeleteAsync(content);
                }
            });
        }
    }
}
