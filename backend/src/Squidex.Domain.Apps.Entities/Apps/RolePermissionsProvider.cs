// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class RolePermissionsProvider
    {
        private readonly List<string> forAppSchemas = new List<string>();
        private readonly List<string> forAppWithoutSchemas = new List<string>();
        private readonly IAppProvider appProvider;

        static RolePermissionsProvider()
        {
        }

        public RolePermissionsProvider(IAppProvider appProvider)
        {
            this.appProvider = appProvider;

            foreach (var field in typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    var value = field.GetValue(null) as string;

                    if (value?.StartsWith(Permissions.App, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        if (value.IndexOf("{schema}", Permissions.App.Length, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            forAppSchemas.Add(value);
                        }
                        else
                        {
                            forAppWithoutSchemas.Add(value);
                        }
                    }
                }
            }
        }

        public async Task<List<string>> GetPermissionsAsync(IAppEntity app)
        {
            var schemaNames = await GetSchemaNamesAsync(app);

            var result = new List<string> { Permission.Any };

            foreach (var permission in forAppWithoutSchemas)
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

            foreach (var permission in forAppSchemas)
            {
                var trimmed = permission[(Permissions.App.Length + 1)..];

                foreach (var schema in schemaNames)
                {
                    var replaced = trimmed.Replace("{schema}", schema, StringComparison.Ordinal);

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
