// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Squidex.Areas.IdentityServer.Views
{
    public static class Extensions
    {
        public static string? RootContentUrl(this IUrlHelper urlHelper, string contentPath)
        {
            if (string.IsNullOrEmpty(contentPath))
            {
                return null;
            }

            if (contentPath[0] == '~')
            {
                var segment = new PathString(contentPath[1..]);

                var applicationPath = urlHelper.ActionContext.HttpContext.Request.PathBase;

                if (applicationPath.Value != null)
                {
                    var indexOfLastPart = applicationPath.Value.LastIndexOf('/');

                    if (indexOfLastPart >= 0)
                    {
                        applicationPath = applicationPath.Value.Substring(0, indexOfLastPart);
                    }
                }

                return applicationPath.Add(segment).Value;
            }

            return contentPath;
        }
    }
}
