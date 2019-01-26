// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public static class DefaultScripts
    {
        private const string ScriptToGenerateSlug = @"
            var data = ctx.data;
    
            if (data.title && data.title.iv) {
                data.slug = { iv: slugify(data.title.iv) };

                replace(data);
            }";

        private const string ScriptToGenerateUsername = @"
            var data = ctx.data;
            
            if (data.userName && data.userName.iv) {
                data.normalizedUserName = { iv: data.userName.iv.toUpperCase() };
            }
            
            if (data.email && data.email.iv) {
                data.normalizedEmail = { iv: data.email.iv.toUpperCase() };
            }

            replace(data);";

        public static readonly Dictionary<string, string> GenerateSlug = new Dictionary<string, string>
        {
            [Scripts.Create] = ScriptToGenerateSlug,
            [Scripts.Update] = ScriptToGenerateSlug
        };

        public static readonly Dictionary<string, string> GenerateUsername = new Dictionary<string, string>
        {
            [Scripts.Create] = ScriptToGenerateUsername,
            [Scripts.Update] = ScriptToGenerateUsername
        };
    }
}
