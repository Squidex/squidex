// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using TestSuite.Fixtures;
using TestSuite.Model;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class ContentReferencesTests : IClassFixture<ContentReferencesFixture>
    {
        public ContentReferencesFixture _ { get; }

        public ContentReferencesTests(ContentReferencesFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_not_deliver_unpublished_references()
        {
            // STEP 1: Create a referenced content.
            var dataA = new TestEntityWithReferencesData();

            var contentA_1 = await _.Contents.CreateAsync(dataA);


            // STEP 2: Create a content with a reference.
            var dataB = new TestEntityWithReferencesData { References = new[] { contentA_1.Id } };

            var contentB_1 = await _.Contents.CreateAsync(dataB);

            await _.Contents.ChangeStatusAsync(contentB_1.Id, "Published");


            // STEP 3: Query new item
            var contentB_2 = await _.Contents.GetAsync(contentB_1.Id);

            Assert.Empty(contentB_2.Data.References);


            // STEP 4: Publish reference
            await _.Contents.ChangeStatusAsync(contentA_1.Id, "Published");


            // STEP 5: Query new item again
            var contentB_3 = await _.Contents.GetAsync(contentB_1.Id);

            Assert.Equal(new string[] { contentA_1.Id }, contentB_3.Data.References);
        }
    }
}
