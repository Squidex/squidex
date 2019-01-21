// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class DefaultAppLogStore : IAppLogStore
    {
        private readonly ILogStore logStore;

        public DefaultAppLogStore(ILogStore logStore)
        {
            Guard.NotNull(logStore, nameof(logStore));

            this.logStore = logStore;
        }

        public Task ReadLogAsync(IAppEntity app, DateTime from, DateTime to, Stream stream)
        {
            Guard.NotNull(app, nameof(app));

            return logStore.ReadLogAsync(app.Id.ToString(), from, to, stream);
        }
    }
}
