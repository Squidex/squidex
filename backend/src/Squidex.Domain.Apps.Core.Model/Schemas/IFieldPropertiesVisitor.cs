// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas;

public interface IFieldPropertiesVisitor<out T, in TArgs>
{
    T Visit(ArrayFieldProperties properties, TArgs args);

    T Visit(AssetsFieldProperties properties, TArgs args);

    T Visit(BooleanFieldProperties properties, TArgs args);

    T Visit(ComponentFieldProperties properties, TArgs args);

    T Visit(ComponentsFieldProperties properties, TArgs args);

    T Visit(DateTimeFieldProperties properties, TArgs args);

    T Visit(GeolocationFieldProperties properties, TArgs args);

    T Visit(JsonFieldProperties properties, TArgs args);

    T Visit(NumberFieldProperties properties, TArgs args);

    T Visit(ReferencesFieldProperties properties, TArgs args);

    T Visit(StringFieldProperties properties, TArgs args);

    T Visit(TagsFieldProperties properties, TArgs args);

    T Visit(UIFieldProperties properties, TArgs args);
}
