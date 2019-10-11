// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Web;

namespace Squidex.ICIS.UI
{
    public sealed class ICISLinkExtension : ICustomLinkExtension
    {
        public void AddLinks(Resource resource, object source)
        {
            if (source is IAppEntity app && app.Name == "commentary")
            {
                resource.AddGetLink("shortcut", "/app/commentary/content/commentary");
            }
        }
    }
}