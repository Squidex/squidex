// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Web.Pipeline
{
    public sealed class AppFeature : IAppFeature
    {
        public NamedId<Guid> AppId { get; }

        public AppFeature(NamedId<Guid> appId)
        {
            AppId = appId;
        }
    }
}
