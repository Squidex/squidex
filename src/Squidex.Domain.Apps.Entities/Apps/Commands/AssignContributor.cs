// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class AssignContributor : AppAggregateCommand
    {
        public string ContributorId { get; set; }

        public AppContributorPermission Permission { get; set; }
    }
}