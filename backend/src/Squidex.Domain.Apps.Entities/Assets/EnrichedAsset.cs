// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;

namespace Squidex.Domain.Apps.Entities.Assets;

public record EnrichedAsset : Asset
{
    public HashSet<string> TagNames { get; set; }

    public string MetadataText { get; set; }

    public string? EditToken { get; set; }
}
