// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Commands;

public abstract class ContentCommand : ContentCommandBase
{
    public DomainId ContentId { get; set; }

    public bool DoNotScript { get; set; }

    public override DomainId AggregateId
    {
        get => DomainId.Combine(AppId, ContentId);
    }
}
