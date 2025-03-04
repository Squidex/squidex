// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure.Json;

namespace Squidex.Providers.SqlServer.App;

public sealed class SqlServerAppDbContext(DbContextOptions options, IJsonSerializer jsonSerializer)
    : AppDbContext(options, jsonSerializer)
{
}
