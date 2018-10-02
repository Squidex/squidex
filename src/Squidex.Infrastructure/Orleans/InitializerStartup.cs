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

namespace Squidex.Infrastructure.Orleans
{
    public sealed class InitializerStartup : IStartupTask
    {
        private readonly IEnumerable<IInitializable> initializables;

        public InitializerStartup(IEnumerable<IInitializable> initializables)
        {
            Guard.NotNull(initializables, nameof(initializables));

            this.initializables = initializables;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            foreach (var initializable in initializables)
            {
                await initializable.InitializeAsync(cancellationToken);
            }
        }
    }
}
