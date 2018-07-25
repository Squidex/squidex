// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IRestoreJob
    {
        Uri Uri { get; }

        Instant Started { get; }

        bool IsFailed { get; }

        string Status { get; }
    }
}
