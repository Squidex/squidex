// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public interface IField
    {
        long Id { get; }

        string Name { get; }

        bool IsLocked { get; }

        bool IsDisabled { get; }

        bool IsHidden { get; }

        FieldProperties RawProperties { get; }

        T Accept<T>(IFieldVisitor<T> visitor);
    }
}