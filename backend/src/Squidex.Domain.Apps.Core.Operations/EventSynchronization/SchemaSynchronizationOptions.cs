// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.EventSynchronization;

public sealed class SchemaSynchronizationOptions
{
    public bool NoFieldDeletion { get; set; }

    public bool NoFieldRecreation { get; set; }
}
