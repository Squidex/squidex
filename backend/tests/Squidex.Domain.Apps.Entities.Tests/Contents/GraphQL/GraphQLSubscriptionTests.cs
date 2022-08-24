// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Infrastructure;
using Squidex.Messaging.Subscriptions;
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
                    id
                  }
                }");

            var (resultTask, context) = ExecuteCoreAsync(new ExecutionOptions { Query = query });

            await context.Resolve<ISubscriptionService>()
                .PublishAsync(new EnrichedAssetEvent
                {
                    Id = id
                });

            var result = await resultTask;

            var expected = new
            {
                data = new
                {
                    assetChanges = new
                    {
                        id
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
                    id
                  }
                }");

            var (resultTask, context) = ExecuteCoreAsync(new ExecutionOptions { Query = query });

            await context.Resolve<ISubscriptionService>()
                .PublishAsync(new EnrichedContentEvent
                {
                    Id = id
                });

            var result = await resultTask;

            var expected = new
            {
                data = new
                {
                    contentChanges = new
                    {
                        id
                    }
                }
            };

            AssertResult(expected, result);
        }
    }
}
