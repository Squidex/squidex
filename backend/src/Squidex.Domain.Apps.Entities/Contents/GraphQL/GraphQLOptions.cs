// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLOptions
    {
        public int CacheDuration { get; set; } = 10 * 60;
    }
}
