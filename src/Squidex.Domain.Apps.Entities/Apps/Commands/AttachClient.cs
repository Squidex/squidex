// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class AttachClient : AppCommand
    {
        public string Id { get; set; }

        public string Secret { get; set; }

        public AttachClient()
        {
            Secret = RandomHash.New();
        }
    }
}
