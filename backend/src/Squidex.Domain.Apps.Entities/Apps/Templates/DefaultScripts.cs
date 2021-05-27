// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        public static readonly SchemaScripts GenerateSlug = new SchemaScripts
        {
            Create = ScriptToGenerateSlug,
            Update = ScriptToGenerateSlug
        };
    }
}
