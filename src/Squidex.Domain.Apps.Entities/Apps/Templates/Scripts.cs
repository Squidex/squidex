// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public static class Scripts
    {
        public const string Slug =
@"var data = ctx.data;
    
if (data.title && data.title.iv) {
    data.slug = { iv: slugify(data.title.iv) };

    replace(data);
}
";
    }
}
