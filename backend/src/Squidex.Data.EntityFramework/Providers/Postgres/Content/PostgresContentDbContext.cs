// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Queries;

namespace Squidex.Providers.Postgres.Content;

public sealed class PostgresContentDbContext(DbContextOptions options, IJsonSerializer jsonSerializer)
    : ContentDbContext(options, jsonSerializer)
{
    public override SqlDialect Dialect => PostgresDialect.Instance;
}
