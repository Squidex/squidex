// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Caching;

public interface ICacheGenerations
{
    Task<string> GetAsync(string key,
        CancellationToken ct = default);

    void Reset(string key);
}
