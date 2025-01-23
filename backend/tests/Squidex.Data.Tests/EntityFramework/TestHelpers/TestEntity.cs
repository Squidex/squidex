// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.EntityFramework.TestHelpers;

public class TestEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public long Number { get; set; }

    public long? Nullable { get; set; }

    public string Text { get; set; }

    public bool Boolean { get; set; }

    public TestJson Json { get; set; }
}

public class TestJson
{
    public long Number { get; set; }

    public long? Nullable { get; set; }

    public string Text { get; set; }

    public bool Boolean { get; set; }

    public long[] Array { get; set; }
}
