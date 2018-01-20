// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.History
{
    public interface IHistoryEventEntity : IEntity
    {
        Guid EventId { get; }

        RefToken Actor { get; }

        string Message { get; }

        long Version { get; }
    }
}
