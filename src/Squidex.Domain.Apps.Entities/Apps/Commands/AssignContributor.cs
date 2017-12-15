// ==========================================================================
//  AssignContributor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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