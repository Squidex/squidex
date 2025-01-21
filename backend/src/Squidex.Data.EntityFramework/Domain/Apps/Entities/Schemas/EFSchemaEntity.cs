// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Schemas;

public sealed class EFSchemaEntity : EFState<Schema>
{
    public DomainId IndexedAppId { get; set; }

    public DomainId IndexedId { get; set; }

    public string IndexedName { get; set; }

    public bool IndexedDeleted { get; set; }

    public override void Prepare()
    {
        IndexedAppId = Document.AppId.Id;
        IndexedDeleted = Document.IsDeleted;
        IndexedId = Document.Id;
        IndexedName = Document.Name;
    }
}
