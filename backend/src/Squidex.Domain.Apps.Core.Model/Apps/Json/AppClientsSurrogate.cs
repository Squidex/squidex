// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class AppClientsSurrogate : Dictionary<string, AppClient>, ISurrogate<AppClients>
    {
        public void FromSource(AppClients source)
        {
            foreach (var (key, client) in source)
            {
                Add(key, client);
            }
        }

        public AppClients ToSource()
        {
            if (Count == 0)
            {
                return AppClients.Empty;
            }

            return new AppClients(this);
        }
    }
}
