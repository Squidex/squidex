// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Commands;

public enum BulkUpdateContentType
{
    Upsert,
    ChangeStatus,
    Create,
    Delete,
    Patch,
    Update,
    Validate
}
