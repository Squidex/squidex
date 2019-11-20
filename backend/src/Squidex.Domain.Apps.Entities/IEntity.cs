// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities
{
    public interface IEntity : IWithId
    {
        Instant Created { get; }

        Instant LastModified { get; }
    }
}