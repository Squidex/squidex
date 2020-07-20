// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Web.Pipeline
{
    public sealed class AppFeature : IAppFeature
    {
        public NamedId<DomainId> AppId { get; }

        public AppFeature(NamedId<DomainId> appId)
        {
            AppId = appId;
        }
    }
}
