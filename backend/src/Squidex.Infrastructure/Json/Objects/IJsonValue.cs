// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure.Json.Objects
{
    public interface IJsonValue : IEquatable<IJsonValue>
    {
        JsonValueType Type { get; }

        bool TryGet(string pathSegment, [MaybeNullWhen(false)] out IJsonValue result);

        string ToJsonString();

        string ToString();
    }
}
