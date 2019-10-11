// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Web
{
    public sealed class NullCustomLinkExtension : ICustomLinkExtension
    {
        public void AddLinks(Resource resource, object source)
        {
        }
    }
}
