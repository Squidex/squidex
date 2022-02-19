// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Text;

#pragma warning disable MA0048 // File name must match type name

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

        public string EnumName { get; }

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
            EnumName = names[$"{fieldName}Enum"];
            LocalizedInputType = names[$"{typeName}InputDto"];
            LocalizedType = names[$"{typeName}Dto"];
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
}
