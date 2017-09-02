// ==========================================================================
//  ScriptUser.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ScriptUser
    {
        public bool IsClient { get; set; }

        public string Id { get; set; }

        public string Email { get; set; }

        public Dictionary<string, string[]> Claims { get; set; }

        public static ScriptUser Create(ClaimsPrincipal principal)
        {
            Guard.NotNull(principal, nameof(principal));

            var subjectId = principal.OpenIdSubject();

            var user = new ScriptUser { IsClient = string.IsNullOrWhiteSpace(subjectId), Email = principal.OpenIdEmail() };

            if (!user.IsClient)
            {
                user.Id = subjectId;
            }
            else
            {
                user.Id = principal.OpenIdClientId();
            }

            user.Claims = principal.Claims.GroupBy(x => x.Type).ToDictionary(x => x.Key, x => x.Select(y => y.Value).ToArray());

            return user;
        }
    }
}