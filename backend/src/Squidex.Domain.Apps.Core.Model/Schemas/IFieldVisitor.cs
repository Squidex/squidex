// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.

namespace Squidex.Domain.Apps.Core.Schemas
{
    public interface IFieldVisitor<out T>
    {
        T Visit(IArrayField field)
        {
            return default;
        }

        T Visit(IField<AssetsFieldProperties> field)
        {
            return default;
        }

        T Visit(IField<BooleanFieldProperties> field)
        {
            return default;
        }

        T Visit(IField<DateTimeFieldProperties> field)
        {
            return default;
        }

        T Visit(IField<GeolocationFieldProperties> field)
        {
            return default;
        }

        T Visit(IField<JsonFieldProperties> field)
        {
            return default;
        }

        T Visit(IField<NumberFieldProperties> field)
        {
            return default;
        }

        T Visit(IField<ReferencesFieldProperties> field)
        {
            return default;
        }

        T Visit(IField<StringFieldProperties> field)
        {
            return default;
        }

        T Visit(IField<TagsFieldProperties> field)
        {
            return default;
        }

        T Visit(IField<UIFieldProperties> field)
        {
            return default;
        }
    }
}
