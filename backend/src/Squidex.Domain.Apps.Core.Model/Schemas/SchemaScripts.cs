// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class SchemaScripts : Freezable
    {
        public static readonly SchemaScripts Empty = new SchemaScripts();

        static SchemaScripts()
        {
            Empty.Freeze();
        }

        public string Change { get; set; }

        public string Create { get; set; }

        public string Update { get; set; }

        public string Delete { get; set; }

        public string Query { get; set; }
    }
}
