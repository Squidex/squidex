// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Migrations
{
    public interface IMigration
    {
        Task UpdateAsync(
            CancellationToken ct);
    }
}
