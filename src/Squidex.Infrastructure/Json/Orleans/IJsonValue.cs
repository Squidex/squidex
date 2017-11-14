// ==========================================================================
//  IJsonValue.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.Json.Orleans
{
    public interface IJsonValue
    {
        object Value { get; }

        bool IsImmutable { get; }
    }
}
