// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Commands;

public sealed class UpdateLanguage : AppCommand
{
    public Language Language { get; set; }

    public bool IsOptional { get; set; }

    public bool IsMaster { get; set; }

    public Language[]? Fallback { get; set; }
}
