// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas;

public interface IFieldSettings
{
    bool IsLocked { get; }

    bool IsDisabled { get; }

    bool IsHidden { get; }
}
