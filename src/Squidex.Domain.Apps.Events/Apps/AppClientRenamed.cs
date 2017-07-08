// ==========================================================================
//  AppClientRenamed.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppClientRenamedEvent")]
    public sealed class AppClientRenamed : AppEvent
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
