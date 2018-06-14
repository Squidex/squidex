// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.GenerateEdmSchema
{
    public sealed class EdmTypeVisitor : IFieldVisitor<IEdmTypeReference>
    {
        private static readonly EdmTypeVisitor Instance = new EdmTypeVisitor();

        private EdmTypeVisitor()
        {
        }

        public static IEdmTypeReference CreateEdmType(IField field)
        {
            return field.Accept(Instance);
        }

        public IEdmTypeReference Visit(IArrayField field)
        {
            return null;
        }

        public IEdmTypeReference Visit(IField<AssetsFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference Visit(IField<BooleanFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.Boolean, field);
        }

        public IEdmTypeReference Visit(IField<DateTimeFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.DateTimeOffset, field);
        }

        public IEdmTypeReference Visit(IField<GeolocationFieldProperties> field)
        {
            return null;
        }

        public IEdmTypeReference Visit(IField<JsonFieldProperties> field)
        {
            return null;
        }

        public IEdmTypeReference Visit(IField<NumberFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.Double, field);
        }

        public IEdmTypeReference Visit(IField<ReferencesFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference Visit(IField<StringFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference Visit(IField<TagsFieldProperties> field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        private static IEdmTypeReference CreatePrimitive(EdmPrimitiveTypeKind kind, IField field)
        {
            return EdmCoreModel.Instance.GetPrimitive(kind, !field.RawProperties.IsRequired);
        }
    }
}
