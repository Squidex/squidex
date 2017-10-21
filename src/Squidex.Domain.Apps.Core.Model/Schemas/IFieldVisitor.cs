// ==========================================================================
//  IFieldVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public interface IFieldVisitor<out T>
    {
        T Visit(AssetsField field);

        T Visit(BooleanField field);

        T Visit(DateTimeField field);

        T Visit(GeolocationField field);

        T Visit(JsonField field);

        T Visit(NumberField field);

        T Visit(ReferencesField field);

        T Visit(StringField field);

        T Visit(TagsField field);
    }
}
