// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;

namespace Squidex.Infrastructure;

public interface IDbContextWithDialect
{
    SqlDialect Dialect { get; }
}
