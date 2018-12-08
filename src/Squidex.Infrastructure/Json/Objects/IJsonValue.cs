// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Json.Objects
{
    public interface IJsonValue : IEquatable<IJsonValue>
    {
        JsonValueType Type { get; }

        string ToJsonString();
    }
}
