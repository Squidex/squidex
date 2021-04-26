// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class AttachClient : AppUpdateCommand
    {
        public string Id { get; set; }

        public string Secret { get; set; }

        public AttachClient()
        {
            Secret = RandomHash.New();
        }
    }
}
