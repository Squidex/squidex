// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Caching;

public interface IRequestCache
{
    void AddDependency(DomainId key, long version);

    void AddDependency<T>(T value);

    void AddHeader(string header);
}
