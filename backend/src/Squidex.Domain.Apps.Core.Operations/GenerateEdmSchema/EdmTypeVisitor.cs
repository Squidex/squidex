// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.GenerateEdmSchema
{
    public sealed class EdmTypeVisitor : IFieldVisitor<IEdmTypeReference?>
    {
        private static readonly EdmComplexType JsonType = new EdmComplexType("Squidex", "Json", null, false, true);
        private readonly EdmTypeFactory typeFactory;

        internal EdmTypeVisitor(EdmTypeFactory typeFactory)
        {
            this.typeFactory = typeFactory;
        }

        public IEdmTypeReference? CreateEdmType(IField field)
        {
            return field.Accept(this);
        }

        public IEdmTypeReference? Visit(IArrayField field)
        {
            var (fieldEdmType, created) = typeFactory($"Data.{field.Name.ToPascalCase()}.Item");

            if (created)
            {
                foreach (var nestedField in field.Fields)
                {
                    var nestedEdmType = nestedField.Accept(this);

                    if (nestedEdmType != null)
                    {
                        fieldEdmType.AddStructuralProperty(nestedField.Name.EscapeEdmField(), nestedEdmType);
                    }
                }
            }

            return new EdmComplexTypeReference(fieldEdmType, false);
        }

        public IEdmTypeReference? Visit(IField<AssetsFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference? Visit(IField<BooleanFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.Boolean, field);
        }

        public IEdmTypeReference? Visit(IField<DateTimeFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.DateTimeOffset, field);
        }

        public IEdmTypeReference? Visit(IField<GeolocationFieldProperties> field)
        {
            return null;
        }

        public IEdmTypeReference? Visit(IField<JsonFieldProperties> field)
        {
            return new EdmComplexTypeReference(JsonType, !field.RawProperties.IsRequired);
        }

        public IEdmTypeReference? Visit(IField<NumberFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.Double, field);
        }

        public IEdmTypeReference? Visit(IField<ReferencesFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference? Visit(IField<StringFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference? Visit(IField<TagsFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference? Visit(IField<UIFieldProperties> field)
        {
            return null;
        }

        private static IEdmTypeReference CreatePrimitive(EdmPrimitiveTypeKind kind, IField field)
        {
            return EdmCoreModel.Instance.GetPrimitive(kind, !field.RawProperties.IsRequired);
        }
    }
}
