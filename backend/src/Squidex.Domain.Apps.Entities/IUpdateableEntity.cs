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
    public interface IUpdateableEntity
    {
        Guid Id { get; set; }

        Instant Created { get; set; }

        Instant LastModified { get; set; }
    }
}
