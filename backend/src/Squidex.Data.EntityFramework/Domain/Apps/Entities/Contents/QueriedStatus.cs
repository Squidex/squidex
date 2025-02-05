// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents;

public class QueriedStatus
{
    public string IndexedSchemaId { get; set; }

    public string Id { get; set; }

    public string Status { get; set; }
}
