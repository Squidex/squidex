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
    public sealed class AppContributorsSurrogate : Dictionary<string, string>, ISurrogate<AppContributors>
    {
        public void FromSource(AppContributors source)
        {
            foreach (var (userId, role) in source)
            {
                Add(userId, role);
            }
        }

        public AppContributors ToSource()
        {
            if (Count == 0)
            {
                return AppContributors.Empty;
            }

            return new AppContributors(this);
        }
    }
}
