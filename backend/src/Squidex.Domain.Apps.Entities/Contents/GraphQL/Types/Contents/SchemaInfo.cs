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

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class SchemaInfo
    {
        public ISchemaEntity Schema { get; }

        public string TypeName { get; }

        public string DisplayName { get; }

        public string ComponentType { get; }

        public string ContentType { get; }

        public string DataType { get; }

        public string DataInputType { get; }

        public string DataFlatType { get; }

        public string ResultType { get; }

        public IReadOnlyList<FieldInfo> Fields { get; }

        private SchemaInfo(ISchemaEntity schema, string typeName, IReadOnlyList<FieldInfo> fields, Names names)
        {
            Schema = schema;
            ComponentType = names[$"{typeName}Component"];
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

            foreach (var schema in schemas.OrderBy(x => x.Created))
            {
                var typeName = schema.TypeName();

                var fieldInfos = new List<FieldInfo>(schema.SchemaDef.Fields.Count);
                var fieldNames = new Names();

                foreach (var field in schema.SchemaDef.Fields.ForApi())
                {
                    fieldInfos.Add(FieldInfo.Build(
                        field,
                        names[$"{typeName}Data{field.TypeName()}"],
                        names,
                        fieldNames));
                }

                yield return new SchemaInfo(schema, typeName, fieldInfos, names);
            }
        }
    }

    internal sealed class FieldInfo
    {
        public static readonly List<FieldInfo> EmptyFields = new List<FieldInfo>();

        public IField Field { get; set; }

        public string FieldName { get; }

        public string FieldNameDynamic { get; }

        public string DisplayName { get; }

        public string LocalizedType { get; }

        public string LocalizedInputType { get; }

        public string NestedType { get; }

        public string NestedInputType { get; }

        public string ComponentType { get; }

        public string ReferenceType { get; }

        public IReadOnlyList<FieldInfo> Fields { get; }

        private FieldInfo(IField field, string typeName, Names names, Names parentNames, IReadOnlyList<FieldInfo> fields)
        {
            var fieldName = parentNames[field.Name.ToCamelCase(), false];

            ComponentType = names[$"{typeName}ComponentUnionDto"];
            DisplayName = field.DisplayName();
            Field = field;
            Fields = fields;
            FieldName = fieldName;
            FieldNameDynamic = names[$"{fieldName}__Dynamic"];
            LocalizedType = names[$"{typeName}Dto"];
            LocalizedInputType = names[$"{typeName}InputDto"];
            NestedInputType = names[$"{typeName}ChildInputDto"];
            NestedType = names[$"{typeName}ChildDto"];
            ReferenceType = names[$"{typeName}UnionDto"];
        }

        public override string ToString()
        {
            return FieldName;
        }

        internal static FieldInfo Build(IRootField rootField, string typeName, Names names, Names parentNames)
        {
            var fieldInfos = EmptyFields;

            if (rootField is IArrayField arrayField)
            {
                var fieldNames = new Names();

                fieldInfos = new List<FieldInfo>(arrayField.Fields.Count);

                foreach (var nestedField in arrayField.Fields.ForApi())
                {
                    fieldInfos.Add(new FieldInfo(
                        nestedField,
                        names[$"{typeName}{nestedField.TypeName()}"],
                        names,
                        fieldNames,
                        EmptyFields));
                }
            }

            return new FieldInfo(rootField, typeName, names, parentNames, fieldInfos);
        }
    }

    internal sealed class Names
    {
        // Reserver names that are used for other GraphQL types.
        private static readonly HashSet<string> ReservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Asset",
            "AssetResultDto",
            "Content",
            "Component",
            "EntityCreatedResultDto",
            "EntitySavedResultDto",
            "JsonScalar",
            "JsonPrimitive",
            "User"
        };
        private readonly Dictionary<string, int> takenNames = new Dictionary<string, int>();

        public string this[string name, bool isEntity = true]
        {
            get => GetName(name, isEntity);
        }

        private string GetName(string name, bool isEntity)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            if (!char.IsLetter(name[0]))
            {
                name = "gql_" + name;
            }
            else if (isEntity && ReservedNames.Contains(name))
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

            // Add + 1 to all offsets for backwards-compatibility.
            return $"{name}{offset + 1}";
        }
    }
}
