// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;
using Squidex.Shared;

namespace Squidex.EntityFramework.TestHelpers;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
public class TestDbContext(DbContextOptions options, IJsonSerializer jsonSerializer) : AppDbContext(options, jsonSerializer)
#pragma warning restore CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.AddSnapshot<SnapshotValue, EFState<SnapshotValue>>(jsonSerializer);
        base.OnModelCreating(builder);
    }
}
