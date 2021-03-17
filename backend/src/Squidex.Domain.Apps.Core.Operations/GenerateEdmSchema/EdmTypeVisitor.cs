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
    internal sealed class EdmTypeVisitor : IFieldVisitor<IEdmTypeReference?, EdmTypeVisitor.Args>
    {
        private static readonly EdmComplexType JsonType = new EdmComplexType("Squidex", "Json", null, false, true);
        private static readonly EdmTypeVisitor Instance = new EdmTypeVisitor();

        public readonly struct Args
        {
            public readonly EdmTypeFactory Factory;

            public Args(EdmTypeFactory factory)
            {
                Factory = factory;
            }
        }

        private EdmTypeVisitor()
        {
        }

        public static IEdmTypeReference? BuildType(IField field, EdmTypeFactory factory)
        {
            var args = new Args(factory);

            return field.Accept(Instance, args);
        }

        public IEdmTypeReference? Visit(IArrayField field, Args args)
        {
            var (fieldEdmType, created) = args.Factory($"Data.{field.Name.ToPascalCase()}.Item");

            if (created)
            {
                foreach (var nestedField in field.Fields)
                {
                    var nestedEdmType = nestedField.Accept(this, args);

                    if (nestedEdmType != null)
                    {
                        fieldEdmType.AddStructuralProperty(nestedField.Name.EscapeEdmField(), nestedEdmType);
                    }
                }
            }

            return new EdmComplexTypeReference(fieldEdmType, false);
        }

        public IEdmTypeReference? Visit(IField<AssetsFieldProperties> field, Args args)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference? Visit(IField<BooleanFieldProperties> field, Args args)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.Boolean, field);
        }

        public IEdmTypeReference? Visit(IField<DateTimeFieldProperties> field, Args args)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.DateTimeOffset, field);
        }

        public IEdmTypeReference? Visit(IField<GeolocationFieldProperties> field, Args args)
        {
            return CreateGeographyPoint(field);
        }

        public IEdmTypeReference? Visit(IField<JsonFieldProperties> field, Args args)
        {
            return CreateJson(field);
        }

        public IEdmTypeReference? Visit(IField<NumberFieldProperties> field, Args args)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.Double, field);
        }

        public IEdmTypeReference? Visit(IField<ReferencesFieldProperties> field, Args args)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference? Visit(IField<StringFieldProperties> field, Args args)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference? Visit(IField<TagsFieldProperties> field, Args args)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference? Visit(IField<UIFieldProperties> field, Args args)
        {
            return null;
        }

        private static IEdmTypeReference CreatePrimitive(EdmPrimitiveTypeKind kind, IField field)
        {
            return EdmCoreModel.Instance.GetPrimitive(kind, !field.RawProperties.IsRequired);
        }

        private static IEdmTypeReference CreateGeographyPoint(IField<GeolocationFieldProperties> field)
        {
            return EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeographyPoint, !field.RawProperties.IsRequired);
        }

        private static IEdmTypeReference CreateJson(IField<JsonFieldProperties> field)
        {
            return new EdmComplexTypeReference(JsonType, !field.RawProperties.IsRequired);
        }
    }
}
