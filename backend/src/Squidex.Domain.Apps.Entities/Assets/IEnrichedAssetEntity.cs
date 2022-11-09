// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Assets;

public interface IEnrichedAssetEntity : IAssetEntity
{
    HashSet<string> TagNames { get; }

    string MetadataText { get; }

    string? EditToken { get; set; }
}
