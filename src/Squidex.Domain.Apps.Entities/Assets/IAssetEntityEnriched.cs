// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetEntityEnriched : IAssetEntity
    {
        HashSet<string> TagNames { get; }
    }
}
