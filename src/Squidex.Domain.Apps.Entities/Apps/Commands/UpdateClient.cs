// ==========================================================================
//  UpdateClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class UpdateClient : AppAggregateCommand
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public AppClientPermission? Permission { get; set; }
    }
}
