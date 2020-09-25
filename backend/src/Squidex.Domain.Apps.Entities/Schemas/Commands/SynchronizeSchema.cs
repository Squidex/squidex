// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Commands;
using SchemaField = Squidex.Domain.Apps.Entities.Schemas.Commands.UpsertSchemaField;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class SynchronizeSchema : SchemaUpdateCommand, IUpsertCommand, IAggregateCommand, ISchemaCommand
    {
        public bool NoFieldDeletion { get; set; }

        public bool NoFieldRecreation { get; set; }

        public bool IsPublished { get; set; }

        public string Category { get; set; }

        public SchemaField[]? Fields { get; set; }

        public FieldNames? FieldsInReferences { get; set; }

        public FieldNames? FieldsInLists { get; set; }

        public FieldRuleCommand[]? FieldRules { get; set; }

        public SchemaScripts? Scripts { get; set; }

        public SchemaProperties Properties { get; set; }

        public Dictionary<string, string>? PreviewUrls { get; set; }

        public Schema BuildSchema(string name, bool isSingleton)
        {
            IUpsertCommand self = this;

            return self.ToSchema(name, isSingleton);
        }
    }
}
