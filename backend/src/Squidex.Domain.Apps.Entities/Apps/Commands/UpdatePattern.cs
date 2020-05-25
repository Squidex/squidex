// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class UpdatePattern : AppUpdateCommand
    {
        public DomainId PatternId { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string? Message { get; set; }
    }
}
