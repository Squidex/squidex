// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Json;

[Serializable]
public class JsonException : Exception
{
    public JsonException()
    {
    }

    public JsonException(string? message)
        : base(message)
    {
    }

    public JsonException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}
