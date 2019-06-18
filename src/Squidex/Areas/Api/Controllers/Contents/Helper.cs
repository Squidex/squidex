// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Areas.Api.Controllers.Contents
{
    public static class Helper
    {
        public static Permission StatusPermission(string app, string schema, Status status)
        {
            var id = Permissions.AppContentsStatus.Replace("{status}", status.ToString());

            return Permissions.ForApp(id, app, schema);
        }

        public static Permission StatusPermission(string app, string schema, Status2 status)
        {
            var id = Permissions.AppContentsStatus.Replace("{status}", status.Name);

            return Permissions.ForApp(id, app, schema);
        }
    }
}
