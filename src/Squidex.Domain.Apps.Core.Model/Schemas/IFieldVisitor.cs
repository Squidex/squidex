// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public interface IFieldVisitor<out T>
    {
        T Visit(IArrayField field);

        T Visit(IField<AssetsFieldProperties> field);

        T Visit(IField<BooleanFieldProperties> field);

        T Visit(IField<DateTimeFieldProperties> field);

        T Visit(IField<GeolocationFieldProperties> field);

        T Visit(IField<JsonFieldProperties> field);

        T Visit(IField<NumberFieldProperties> field);

        T Visit(IField<ReferencesFieldProperties> field);

        T Visit(IField<StringFieldProperties> field);

        T Visit(IField<TagsFieldProperties> field);
    }
}
