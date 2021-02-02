// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class RolePermissionsProvider
    {
        private readonly IAppProvider appProvider;

        public RolePermissionsProvider(IAppProvider appProvider)
        {
            Guard.NotNull(appProvider, nameof(appProvider));

            this.appProvider = appProvider;
        }

        public async Task<List<string>> GetPermissionsAsync(IAppEntity app)
        {
            var schemaNames = await GetSchemaNamesAsync(app);

            var result = new List<string> { Permission.Any };

            foreach (var permission in Permissions.ForAppsNonSchema)
            {
                if (permission.Length > Permissions.App.Length + 1)
                {
                    var trimmed = permission[(Permissions.App.Length + 1)..];

                    if (trimmed.Length > 0)
                    {
                        result.Add(trimmed);
                    }
                }
            }

            foreach (var permission in Permissions.ForAppsSchema)
            {
                var trimmed = permission[(Permissions.App.Length + 1)..];

                foreach (var schema in schemaNames)
                {
                    var replaced = trimmed.Replace("{name}", schema);

                    result.Add(replaced);
                }
            }

            return result;
        }

        private async Task<List<string>> GetSchemaNamesAsync(IAppEntity app)
        {
            var schemas = await appProvider.GetSchemasAsync(app.Id);

            var schemaNames = new List<string>
            {
                Permission.Any
            };

            schemaNames.AddRange(schemas.Select(x => x.SchemaDef.Name));

            return schemaNames;
        }
    }
}
