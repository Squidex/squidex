// ==========================================================================
//  AttachClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class AttachClient : AppAggregateCommand
    {
        public string Id { get; set; }

        public string Secret { get; } = RandomHash.New();
    }
}
