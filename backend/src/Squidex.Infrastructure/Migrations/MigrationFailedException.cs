// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Migrations;

[Serializable]
public class MigrationFailedException(string name, Exception? inner = null) : Exception(FormatException(name), inner)
{
    public string Name { get; } = name;

    private static string FormatException(string name)
    {
        return $"Failed to run migration '{name}'";
    }
}
