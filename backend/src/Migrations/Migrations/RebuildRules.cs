// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations
{
    public sealed class RebuildRules : IMigration
    {
        private readonly Rebuilder rebuilder;
        private readonly RebuildOptions rebuildOptions;

        public RebuildRules(Rebuilder rebuilder,
            IOptions<RebuildOptions> rebuildOptions)
        {
            this.rebuilder = rebuilder;
            this.rebuildOptions = rebuildOptions.Value;
        }

        public Task UpdateAsync(
            CancellationToken ct)
        {
            return rebuilder.RebuildRulesAsync(rebuildOptions.BatchSize, ct);
        }
    }
}
