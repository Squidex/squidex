// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class UpdateClient : AppCommand
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public AppClientPermission? Permission { get; set; }
    }
}
