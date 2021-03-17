// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.ClientLibrary;
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

            var contentB_1 = await _.Contents.CreateAsync(dataB, true);


            // STEP 3: Query new item
            var contentB_2 = await _.Contents.GetAsync(contentB_1.Id);

            Assert.Empty(contentB_2.Data.References);


            // STEP 4: Publish reference
            await _.Contents.ChangeStatusAsync(contentA_1.Id, "Published");


            // STEP 5: Query new item again
            var contentB_3 = await _.Contents.GetAsync(contentB_1.Id);

            Assert.Equal(new string[] { contentA_1.Id }, contentB_3.Data.References);
        }

        [Fact]
        public async Task Should_not_delete_when_referenced()
        {
            // STEP 1: Create a referenced content.
            var dataA = new TestEntityWithReferencesData();

            var contentA_1 = await _.Contents.CreateAsync(dataA, true);


            // STEP 2: Create a content with a reference.
            var dataB = new TestEntityWithReferencesData { References = new[] { contentA_1.Id } };

            await _.Contents.CreateAsync(dataB, true);


            // STEP 3: Try to delete with referrer check.
            await Assert.ThrowsAsync<SquidexException>(() => _.Contents.DeleteAsync(contentA_1.Id, checkReferrers: true));


            // STEP 4: Delete without referrer check
            await _.Contents.DeleteAsync(contentA_1.Id, checkReferrers: false);
        }

        [Fact]
        public async Task Should_not_unpublish_when_referenced()
        {
            // STEP 1: Create a published referenced content.
            var dataA = new TestEntityWithReferencesData();

            var contentA_1 = await _.Contents.CreateAsync(dataA, true);


            // STEP 2: Create a content with a reference.
            var dataB = new TestEntityWithReferencesData { References = new[] { contentA_1.Id } };

            await _.Contents.CreateAsync(dataB, true);


            // STEP 3: Try to delete with referrer check.
            await Assert.ThrowsAsync<SquidexException>(() => _.Contents.ChangeStatusAsync(contentA_1.Id, new ChangeStatus
            {
                Status = "Draft",
                CheckReferrers = true
            }));


            // STEP 4: Delete without referrer check
            await _.Contents.ChangeStatusAsync(contentA_1.Id, new ChangeStatus
            {
                Status = "Draft",
                CheckReferrers = false
            });
        }

        [Fact]
        public async Task Should_not_delete_with_bulk_when_referenced()
        {
            // STEP 1: Create a referenced content.
            var dataA = new TestEntityWithReferencesData();

            var contentA_1 = await _.Contents.CreateAsync(dataA, true);


            // STEP 2: Create a content with a reference.
            var dataB = new TestEntityWithReferencesData { References = new[] { contentA_1.Id } };

            await _.Contents.CreateAsync(dataB, true);


            // STEP 3: Try to delete with referrer check.
            var result1 = await _.Contents.BulkUpdateAsync(new BulkUpdate
            {
                Jobs = new List<BulkUpdateJob>
                {
                    new BulkUpdateJob
                    {
                        Id = contentA_1.Id,
                        Type = BulkUpdateType.Delete,
                        Status = "Draft"
                    }
                },
                CheckReferrers = true
            });

            Assert.NotNull(result1[0].Error);


            // STEP 4: Delete without referrer check
            var result2 = await _.Contents.BulkUpdateAsync(new BulkUpdate
            {
                Jobs = new List<BulkUpdateJob>
                {
                    new BulkUpdateJob
                    {
                        Id = contentA_1.Id,
                        Type = BulkUpdateType.Delete,
                        Status = "Draft"
                    }
                },
                CheckReferrers = false
            });

            Assert.Null(result2[0].Error);
        }

        [Fact]
        public async Task Should_not_unpublish_with_bulk_when_referenced()
        {
            // STEP 1: Create a published referenced content.
            var dataA = new TestEntityWithReferencesData();

            var contentA_1 = await _.Contents.CreateAsync(dataA, true);


            // STEP 2: Create a published content with a reference.
            var dataB = new TestEntityWithReferencesData { References = new[] { contentA_1.Id } };

            await _.Contents.CreateAsync(dataB, true);


            // STEP 3: Try to delete with referrer check.
            var result1 = await _.Contents.BulkUpdateAsync(new BulkUpdate
            {
                Jobs = new List<BulkUpdateJob>
                {
                    new BulkUpdateJob
                    {
                        Id = contentA_1.Id,
                        Type = BulkUpdateType.ChangeStatus,
                        Status = "Draft"
                    }
                },
                CheckReferrers = true
            });

            Assert.NotNull(result1[0].Error);


            // STEP 4: Delete without referrer check
            var result2 = await _.Contents.BulkUpdateAsync(new BulkUpdate
            {
                Jobs = new List<BulkUpdateJob>
                {
                    new BulkUpdateJob
                    {
                        Id = contentA_1.Id,
                        Type = BulkUpdateType.ChangeStatus,
                        Status = "Draft"
                    }
                },
                CheckReferrers = false
            });

            Assert.Null(result2[0].Error);
        }
    }
}
