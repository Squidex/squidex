// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Migrations
{
    public interface IMigrationPath
    {
        (int Version, IEnumerable<IMigration>? Migrations) GetNext(int version);
    }
}
