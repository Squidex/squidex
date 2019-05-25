// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class UpsertAppPattern
    {
        public string Name { get; set; }

        public string Pattern { get; set; }

        public string Message { get; set; }
    }
}
