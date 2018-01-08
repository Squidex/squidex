// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Migrations
{
    public interface IMigration
    {
        int FromVersion { get; }

        int ToVersion { get; }

        Task UpdateAsync(IEnumerable<IMigration> previousMigrations);
    }
}
