// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans.Runtime;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class InitializerStartup : IStartupTask
    {
        private readonly IEnumerable<IInitializable> targets;
        private readonly ISemanticLog log;

        public InitializerStartup(IEnumerable<IInitializable> targets, ISemanticLog log)
        {
            Guard.NotNull(targets, nameof(targets));

            this.targets = targets;

            this.log = log;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            foreach (var target in targets)
            {
                await target.InitializeAsync(cancellationToken);

                log?.LogInformation(w => w.WriteProperty("siloInitializedSystem", target.GetType().Name));
            }
        }
    }
}
