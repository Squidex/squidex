// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure.Log;

public sealed class Request
{
    public Instant Timestamp { get; set; }

    public string Key { get; set; }

    public Dictionary<string, string> Properties { get; set; } = [];
}
