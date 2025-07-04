﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Templates.Models;

public sealed class TemplateDto : Resource
{
    /// <summary>
    /// The name of the template.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The title of the template.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The description of the template.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The details of the template.
    /// </summary>
    public string Details { get; set; }

    /// <summary>
    /// True, if the template is a starter.
    /// </summary>
    public bool IsStarter { get; set; }

    /// <summary>
    /// The optional logo.
    /// </summary>
    public string? Logo { get; set; }

    public static TemplateDto FromDomain(Template template, Resources resources)
    {
        var result = SimpleMapper.Map(template, new TemplateDto());

        return result.CreateLinks(resources);
    }

    private TemplateDto CreateLinks(Resources resources)
    {
        var values = new { name = Name };

        AddSelfLink(resources.Url<TemplatesController>(c => nameof(c.GetTemplate), values));

        return this;
    }
}
