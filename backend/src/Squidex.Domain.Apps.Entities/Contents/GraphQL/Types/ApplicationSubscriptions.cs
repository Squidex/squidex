// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

internal sealed class ApplicationSubscriptions : ObjectGraphType
{
    public ApplicationSubscriptions()
    {
        AddField(new FieldType
        {
            Name = $"assetChanges",
            Arguments = AssetActions.Subscription.Arguments,
            ResolvedType = SharedTypes.EnrichedAssetEvent,
            Resolver = null,
            StreamResolver = AssetActions.Subscription.Resolver,
            Description = "Subscribe to asset events."
        });

        AddField(new FieldType
        {
            Name = $"contentChanges",
            Arguments = ContentActions.Subscription.Arguments,
            ResolvedType = SharedTypes.EnrichedContentEvent,
            Resolver = null,
            StreamResolver = ContentActions.Subscription.Resolver,
            Description = "Subscribe to content events."
        });
    }
}
