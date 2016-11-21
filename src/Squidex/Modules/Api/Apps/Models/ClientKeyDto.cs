// ==========================================================================
//  ClientKeyDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Modules.Api.Apps.Models
{
    public sealed class ClientKeyDto
    {
        public string ClientKey { get; set; }

        public DateTime ExpiresUtc { get; set; }
    }
}
