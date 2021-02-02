// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Text;

#pragma warning disable SA1649 // File name should match first type name

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class SchemaInfo
    {
        public ISchemaEntity Schema { get; }

        public string TypeName { get; }

        public string DisplayName { get; }

        public string ContentType { get; }

        public string DataType { get; }

        public string DataInputType { get; }

        public string DataFlatType { get; }

        public string ResultType { get; }

        public IReadOnlyList<FieldInfo> Fields { get; }

        private SchemaInfo(ISchemaEntity schema, string typeName, IReadOnlyList<FieldInfo> fields, Names names)
        {
            Schema = schema;
            ContentType = names[typeName];
            DataFlatType = names[$"{typeName}FlatDataDto"];
            DataInputType = names[$"{typeName}DataInputDto"];
            ResultType = names[$"{typeName}ResultDto"];
            DataType = names[$"{typeName}DataDto"];
            DisplayName = schema.DisplayName();
            Fields = fields;
            TypeName = typeName;
        }

        public override string ToString()
        {
            return TypeName;
        }

        public static IEnumerable<SchemaInfo> Build(IEnumerable<ISchemaEntity> schemas)
        {
            var names = new Names();

            foreach (var schema in schemas.Where(x => x.SchemaDef.IsPublished && x.SchemaDef.Fields.Count > 0).OrderBy(x => x.Created))
            {
                var typeName = schema.TypeName();

                var fields = FieldInfo.EmptyFields;

                if (schema.SchemaDef.Fields.Count > 0)
                {
                    var fieldNames = new Names();

                    fields = new List<FieldInfo>(schema.SchemaDef.Fields.Count);

                    foreach (var field in schema.SchemaDef.Fields)
                    {
                        fields.Add(FieldInfo.Build(field, fieldNames[field], names[$"{typeName}Data{field.TypeName()}"], names));
                    }
                }

                yield return new SchemaInfo(schema, typeName, fields, names);
            }
        }
    }

    internal sealed class FieldInfo
    {
        public static readonly List<FieldInfo> EmptyFields = new List<FieldInfo>();

        public IField Field { get; set; }

        public string FieldName { get; }

        public string DisplayName { get; }

        public string LocalizedType { get; }

        public string LocalizedInputType { get; }

        public string NestedType { get; }

        public string NestedInputType { get; }

        public string UnionType { get; }

        public IReadOnlyList<FieldInfo> Fields { get; }

        private FieldInfo(IField field, string fieldName, string typeName, IReadOnlyList<FieldInfo> fields, Names names)
        {
            DisplayName = field.DisplayName();
            Field = field;
            Fields = fields;
            FieldName = fieldName;
            LocalizedType = names[$"{typeName}Dto"];
            LocalizedInputType = names[$"{typeName}InputDto"];
            NestedInputType = names[$"{typeName}ChildInputDto"];
            NestedType = names[$"{typeName}ChildDto"];
            UnionType = names[$"{typeName}UnionDto"];
        }

        public override string ToString()
        {
            return FieldName;
        }

        internal static FieldInfo Build(IRootField rootField, string fieldName, string typeName, Names names)
        {
            var fields = EmptyFields;

            if (rootField is IArrayField arrayField && arrayField.Fields.Count > 0)
            {
                var fieldNames = new Names();

                fields = new List<FieldInfo>(arrayField.Fields.Count);

                foreach (var field in arrayField.Fields)
                {
                    fields.Add(new FieldInfo(field, fieldNames[field], $"{typeName}{field.TypeName()}", EmptyFields, names));
                }
            }

            return new FieldInfo(rootField, fieldName, typeName, fields, names);
        }
    }

    internal sealed class Names
    {
        private readonly Dictionary<string, int> takenNames = new Dictionary<string, int>();

        public string this[IField field]
        {
            get
            {
                return this[field.Name.ToCamelCase()];
            }
        }

        public string this[string name]
        {
            get
            {
                Guard.NotNullOrEmpty(name, nameof(name));

                if (!char.IsLetter(name[0]))
                {
                    name = "gql_" + name;
                }
                else if (name.Equals("Content", StringComparison.OrdinalIgnoreCase))
                {
                    name = $"{name}Entity";
                }

                // Avoid duplicate names.
                if (!takenNames.TryGetValue(name, out var offset))
                {
                    takenNames[name] = 0;
                    return name;
                }

                takenNames[name] = ++offset;

                // Add + 1 to all offset for backwars compatibility.
                return $"{name}{offset + 1}";
            }
        }
    }
}