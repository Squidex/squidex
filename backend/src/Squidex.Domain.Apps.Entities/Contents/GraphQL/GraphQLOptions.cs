// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public sealed class GraphQLOptions
{
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

    public int DataLoaderBatchSize { get; set; } = 1000;

    public bool EnableSubscriptions { get; set; } = true;
}
