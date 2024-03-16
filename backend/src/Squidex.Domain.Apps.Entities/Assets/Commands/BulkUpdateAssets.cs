// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class BulkUpdateAssets : SquidexCommand, IAppCommand
    {
        public NamedId<DomainId> AppId { get; set; }

        public bool CheckReferrers { get; set; }

        public BulkUpdateJob[]? Jobs { get; set; }
    }
}
