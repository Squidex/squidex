// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Commands
{
    public record CommandResult(DomainId Id, long NewVersion, long OldVersion, object Payload)
    {
        public bool IsCreated => OldVersion < 0;

        public bool IsChanged => OldVersion != NewVersion;

        public CommandResult(DomainId id, long newVersion, long oldVersion)
            : this(id, newVersion, oldVersion, None.Value)
        {
        }
    }
}
