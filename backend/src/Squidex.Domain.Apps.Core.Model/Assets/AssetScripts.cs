// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Assets
{
    public sealed record AssetScripts
    {
        public static readonly AssetScripts Empty = new AssetScripts();

        public string? Create { get; init; }

        public string? Update { get; init; }

        public string? Annotate { get; init; }

        public string? Move { get; init; }

        public string? Delete { get; init; }
    }
}
