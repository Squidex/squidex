// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Domain.Apps.Entities
{
    public interface IEntity
    {
        Guid Id { get; }

        Instant Created { get; }

        Instant LastModified { get; }
    }
}