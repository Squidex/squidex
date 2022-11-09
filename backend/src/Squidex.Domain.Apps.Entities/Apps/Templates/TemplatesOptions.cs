// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

public sealed class TemplatesOptions
{
    public string? LocalUrl { get; set; }

    public TemplateRepository[] Repositories { get; set; }
}
