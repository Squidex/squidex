// ==========================================================================
//  IMongoEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Infrastructure.MongoDb
{
    public interface IMongoEntity
    {
        Guid Id { get; set; }

        Instant Created { get; set; }

        Instant LastModified { get; set; }
    }
}
