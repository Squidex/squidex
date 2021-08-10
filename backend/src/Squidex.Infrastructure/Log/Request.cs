// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using NodaTime;

#pragma warning disable SA1401 // Fields should be private

namespace Squidex.Infrastructure.Log
{
    public sealed class Request
    {
        public Instant Timestamp;

        public string Key;

        public Dictionary<string, string> Properties = new Dictionary<string, string>();
    }
}
