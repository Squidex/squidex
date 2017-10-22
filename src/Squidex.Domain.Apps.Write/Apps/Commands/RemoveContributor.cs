// ==========================================================================
//  RemoveContributor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class RemoveContributor : AppAggregateCommand
    {
        public string ContributorId { get; set; }
    }
}