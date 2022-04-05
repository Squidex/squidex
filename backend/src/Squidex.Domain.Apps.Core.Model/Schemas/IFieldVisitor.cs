// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public interface IFieldVisitor<out T, in TArgs>
    {
        T Visit(IArrayField field, TArgs args);

        T Visit(IField<AssetsFieldProperties> field, TArgs args);

        T Visit(IField<BooleanFieldProperties> field, TArgs args);

        T Visit(IField<ComponentFieldProperties> field, TArgs args);

        T Visit(IField<ComponentsFieldProperties> field, TArgs args);

        T Visit(IField<DateTimeFieldProperties> field, TArgs args);

        T Visit(IField<GeolocationFieldProperties> field, TArgs args);

        T Visit(IField<JsonFieldProperties> field, TArgs args);

        T Visit(IField<NumberFieldProperties> field, TArgs args);

        T Visit(IField<ReferencesFieldProperties> field, TArgs args);

        T Visit(IField<StringFieldProperties> field, TArgs args);

        T Visit(IField<TagsFieldProperties> field, TArgs args);

        T Visit(IField<UIFieldProperties> field, TArgs args);
    }
}
