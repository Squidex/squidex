// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Infrastructure.Migrations;

public sealed class EFMigrationEntity
{
    [Key]
    public int Id { get; set; }

    public bool IsLocked { get; set; }

    public int Version { get; set; }
}
