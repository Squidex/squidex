// ==========================================================================
//  ScriptUser.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ScriptUser
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public bool IsClient { get; set; }

        public Dictionary<string, string[]> Scopes { get; } = new Dictionary<string, string[]>();

        public static ScriptUser Create(ClaimsPrincipal principal)
        {
            return new ScriptUser();
        }
    }
}