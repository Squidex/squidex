// =========================================================================
//  AppContributorDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Apps;

namespace Squidex.Modules.Api.Apps.Models
{
    public sealed class AppContributorDto
    {
        public string ContributorId { get; set; }

        public PermissionLevel Permission { get; set; }
    }
}
