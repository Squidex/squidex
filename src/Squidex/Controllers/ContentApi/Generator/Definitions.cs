// ==========================================================================
//  Definitions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using NSwag;
using Squidex.Config;
using Squidex.Pipeline.Swagger;
using Squidex.Shared.Identity;

namespace Squidex.Controllers.ContentApi.Generator
{
    public static class Definitions
    {
        public static readonly string SchemaQueryDescription;
        public static readonly string SchemaBodyDescription;
        public static readonly List<SwaggerSecurityRequirement> EditorSecurity;
        public static readonly List<SwaggerSecurityRequirement> ReaderSecurity;

        static Definitions()
        {
            SchemaBodyDescription = SwaggerHelper.LoadDocs("schemabody");
            SchemaQueryDescription = SwaggerHelper.LoadDocs("schemaquery");

            ReaderSecurity = new List<SwaggerSecurityRequirement>
            {
                new SwaggerSecurityRequirement
                {
                    {
                        Constants.SecurityDefinition, new[] { SquidexRoles.AppReader }
                    }
                }
            };

            EditorSecurity = new List<SwaggerSecurityRequirement>
            {
                new SwaggerSecurityRequirement
                {
                    {
                        Constants.SecurityDefinition, new[] { SquidexRoles.AppEditor }
                    }
                }
            };
        }
    }
}
