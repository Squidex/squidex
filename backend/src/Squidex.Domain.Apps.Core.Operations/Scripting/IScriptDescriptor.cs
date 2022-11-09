// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.Scripting;

public delegate void AddDescription(JsonType type, string name, string description);

public interface IScriptDescriptor
{
    void Describe(AddDescription describe, ScriptScope scope);
}
