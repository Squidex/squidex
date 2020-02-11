// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Model;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class ContentUpdateTests : IClassFixture<ContentFixture>
    {
        public ContentFixture _ { get; }

        public ContentUpdateTests(ContentFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_create_strange_text()
        {
            const string text = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";

            TestEntity content = null;
            try
            {
                content = await _.Contents.CreateAsync(new TestEntityData { String = text }, true);

                var updated = await _.Contents.GetAsync(content.Id);

                Assert.Equal(text, updated.Data.String);
            }
            finally
            {
                if (content != null)
                {
                    await _.Contents.DeleteAsync(content.Id);
                }
            }
        }

        [Fact]
        public async Task Should_not_return_not_published_item()
        {
            TestEntity content = null;
            try
            {
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 });

                await Assert.ThrowsAsync<SquidexException>(() => _.Contents.GetAsync(content.Id));
            }
            finally
            {
                if (content != null)
                {
                    await _.Contents.DeleteAsync(content.Id);
                }
            }
        }

        [Fact]
        public async Task Should_return_item_published_with_creation()
        {
            TestEntity content = null;
            try
            {
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, true);

                await _.Contents.GetAsync(content.Id);
            }
            finally
            {
                if (content != null)
                {
                    await _.Contents.DeleteAsync(content.Id);
                }
            }
        }

        [Fact]
        public async Task Should_return_item_published_item()
        {
            TestEntity content = null;
            try
            {
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 });

                await _.Contents.ChangeStatusAsync(content.Id, Status.Published);
                await _.Contents.GetAsync(content.Id);
            }
            finally
            {
                if (content != null)
                {
                    await _.Contents.DeleteAsync(content.Id);
                }
            }
        }

        [Fact]
        public async Task Should_not_return_archived_item()
        {
            TestEntity content = null;
            try
            {
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, true);

                await _.Contents.ChangeStatusAsync(content.Id, Status.Archived);

                await Assert.ThrowsAsync<SquidexException>(() => _.Contents.GetAsync(content.Id));
            }
            finally
            {
                if (content != null)
                {
                    await _.Contents.DeleteAsync(content.Id);
                }
            }
        }

        [Fact]
        public async Task Should_not_return_unpublished_item()
        {
            TestEntity content = null;
            try
            {
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 });

                await _.Contents.ChangeStatusAsync(content.Id, Status.Published);
                await _.Contents.ChangeStatusAsync(content.Id, Status.Draft);

                await Assert.ThrowsAsync<SquidexException>(() => _.Contents.GetAsync(content.Id));
            }
            finally
            {
                if (content != null)
                {
                    await _.Contents.DeleteAsync(content.Id);
                }
            }
        }

        [Fact]
        public async Task Should_update_item()
        {
            TestEntity content = null;
            try
            {
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 2 }, true);

                await _.Contents.UpdateAsync(content.Id, new TestEntityData { Number = 2 });

                var updated = await _.Contents.GetAsync(content.Id);

                Assert.Equal(2, updated.Data.Number);
            }
            finally
            {
                if (content != null)
                {
                    await _.Contents.DeleteAsync(content.Id);
                }
            }
        }

        [Fact]
        public async Task Should_patch_item()
        {
            TestEntity content = null;
            try
            {
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, true);

                await _.Contents.PatchAsync(content.Id, new TestEntityData { Number = 2 });

                var updated = await _.Contents.GetAsync(content.Id);

                Assert.Equal(2, updated.Data.Number);
            }
            finally
            {
                if (content != null)
                {
                    await _.Contents.DeleteAsync(content.Id);
                }
            }
        }

        [Fact]
        public async Task Should_delete_item()
        {
            var content = await _.Contents.CreateAsync(new TestEntityData { Number = 2 }, true);

            await _.Contents.DeleteAsync(content.Id);

            var updated = await _.Contents.GetAsync();

            Assert.DoesNotContain(updated.Items, x => x.Id == content.Id);
        }
    }
}
