// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Migrations;

[Serializable]
public class MigrationFailedException : Exception
{
    public string Name { get; }

    public MigrationFailedException(string name, Exception? inner = null)
        : base(FormatException(name), inner)
    {
        Name = name;
    }

    private static string FormatException(string name)
    {
        return $"Failed to run migration '{name}'";
    }
}
