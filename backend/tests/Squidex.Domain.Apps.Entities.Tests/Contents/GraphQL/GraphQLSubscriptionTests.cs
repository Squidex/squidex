// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reactive.Linq;
using FakeItEasy;
using GraphQL;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public class GraphQLSubscriptionTests : GraphQLTestBase
    {
        [Fact]
        public async Task Should_subscribe_to_assets()
        {
            var id = DomainId.NewGuid();

            var query = CreateQuery(@"
                subscription {
                  assetChanges {
                    id,
                    fileName,
                    fileSize
                  }
                }");

            var stream =
                Observable.Return<object>(
                    new EnrichedAssetEvent
                    {
                        Id = id,
                        FileName = "image.png",
                        FileSize = 1024
                    });

            A.CallTo(() => subscriptionService.Subscribe<object>(A<AssetSubscription>._))
                .Returns(stream);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    assetChanges = new
                    {
                        id,
                        fileName = "image.png",
                        fileSize = 1024
                    }
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_subscribe_to_contents()
        {
            var id = DomainId.NewGuid();

            var query = CreateQuery(@"
                subscription {
                  contentChanges {
                    id,
                    data
                  }
                }");

            var stream =
                Observable.Return<object>(
                    new EnrichedContentEvent
                    {
                        Id = id,
                        Data = new ContentData()
                            .AddField("field",
                                new ContentFieldData()
                                    .AddInvariant(42))
                    });

            A.CallTo(() => subscriptionService.Subscribe<object>(A<ContentSubscription>._))
                .Returns(stream);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    contentChanges = new
                    {
                        id,
                        data = new
                        {
                            field = new
                            {
                                iv = 42
                            }
                        }
                    }
                }
            };

            AssertResult(expected, result);
        }
    }
}
