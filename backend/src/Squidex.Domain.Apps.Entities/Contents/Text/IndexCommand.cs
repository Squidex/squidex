// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public abstract class IndexCommand
{
    public UniqueContentId UniqueContentId { get; set; }

    public NamedId<DomainId> SchemaId { get; set; }

    public byte Stage { get; set; }

    public string ToDocId()
    {
        return $"{UniqueContentId.AppId}__{UniqueContentId.ContentId}_{Stage}";
    }
}
