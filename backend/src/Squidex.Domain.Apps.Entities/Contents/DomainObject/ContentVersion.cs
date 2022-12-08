// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject;

public sealed class ContentVersion
{
    public Status Status { get; }

    public ContentData Data { get; }

    public ContentVersion(Status status, ContentData data)
    {
        Guard.NotNull(data);

        Status = status;

        Data = data;
    }

    public ContentVersion WithStatus(Status status)
    {
        return new ContentVersion(status, Data);
    }

    public ContentVersion WithData(ContentData data)
    {
        return new ContentVersion(Status, data);
    }
}
