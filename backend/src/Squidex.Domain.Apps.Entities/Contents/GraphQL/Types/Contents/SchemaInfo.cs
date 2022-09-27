// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Text;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class SchemaInfo
    {
        public ISchemaEntity Schema { get; }

        public string DisplayName => Schema.DisplayName();

        public string ComponentType { get; }

        public string ContentType { get; }

        public string DataFlatType { get; }

        public string DataInputType { get; }

        public string DataType { get; }

        public string ContentResultType { get; }

        public string TypeName { get; }

        public IReadOnlyList<FieldInfo> Fields { get; init; }

        private SchemaInfo(ISchemaEntity schema, string typeName, TypeNames rootScope)
        {
            Schema = schema;

            ComponentType = rootScope[$"{typeName}Component"];
            ContentResultType = rootScope[$"{typeName}ResultDto"];
            ContentType = typeName;
            DataFlatType = rootScope[$"{typeName}FlatDataDto"];
            DataInputType = rootScope[$"{typeName}DataInputDto"];
            DataType = rootScope[$"{typeName}DataDto"];
            TypeName = typeName;
        }

        public override string ToString()
        {
            return TypeName;
        }

        public static IEnumerable<SchemaInfo> Build(IEnumerable<ISchemaEntity> schemas, TypeNames rootScope)
        {
            foreach (var schema in schemas.OrderBy(x => x.Created))
            {
                var typeName = rootScope[schema.TypeName()];

                yield return new SchemaInfo(schema, typeName, rootScope)
                {
                    Fields = FieldInfo.Build(schema.SchemaDef.Fields, $"{typeName}Data", rootScope).ToList()
                };
            }
        }
    }

    internal sealed class FieldInfo
    {
        public IField Field { get; set; }

        public string DisplayName => Field.DisplayName();

        public string EmbeddableStringType { get; }

        public string EmbeddedEnumType { get; }

        public string FieldName { get; }

        public string FieldNameDynamic { get; }

        public string LocalizedInputType { get; }

        public string LocalizedType { get; }

        public string LocalizedTypeDynamic { get; }

        public string NestedInputType { get; }

        public string NestedType { get; }

        public string UnionComponentType { get; }

        public string UnionReferenceType { get; }

        public IReadOnlyList<FieldInfo> Fields { get; init; }

        private FieldInfo(IField field, string fieldName, string typeName, TypeNames rootScope)
        {
            Field = field;

            EmbeddableStringType = rootScope[$"{typeName}EmbeddableString"];
            EmbeddedEnumType = rootScope[$"{typeName}Enum"];
            FieldName = fieldName;
            FieldNameDynamic = $"{fieldName}__Dynamic";
            LocalizedInputType = rootScope[$"{typeName}InputDto"];
            LocalizedType = rootScope[$"{typeName}Dto"];
            LocalizedTypeDynamic = rootScope[$"{typeName}Dto__Dynamic"];
            NestedInputType = rootScope[$"{typeName}ChildInputDto"];
            NestedType = rootScope[$"{typeName}ChildDto"];
            UnionComponentType = rootScope[$"{typeName}ComponentUnionDto"];
            UnionReferenceType = rootScope[$"{typeName}UnionDto"];
        }

        public override string ToString()
        {
            return FieldName;
        }

        internal static IEnumerable<FieldInfo> Build(IEnumerable<IField> fields, string typeName, TypeNames rootScope)
        {
            var typeScope = new TypeNames();

            foreach (var field in fields.ForApi())
            {
                // Field names must be unique within the scope of the parent type.
                var fieldName = typeScope[field.Name.ToCamelCase(), false];

                // Type names must be globally unique.
                var fieldTypeName = rootScope[$"{typeName}{field.TypeName()}"];

                var nested = new List<FieldInfo>();

                if (field is IArrayField arrayField)
                {
                    nested = Build(arrayField.Fields, fieldTypeName, rootScope).ToList();
                }

                yield return new FieldInfo(
                    field,
                    fieldName,
                    fieldTypeName,
                    rootScope)
                {
                    Fields = nested
                };
            }
        }
    }
}
