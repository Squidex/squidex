// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class SynchronizeSchema : UpsertCommand
    {
        public bool NoFieldDeletion { get; set; }

        public bool NoFieldRecreation { get; set; }
    }
}
