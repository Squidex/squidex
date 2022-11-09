// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Templates.Models;

public sealed class TemplatesDto : Resource
{
    /// <summary>
    /// The event consumers.
    /// </summary>
    public TemplateDto[] Items { get; set; }

    public static TemplatesDto FromDomain(IEnumerable<Template> items, Resources resources)
    {
        var result = new TemplatesDto
        {
            Items = items.Select(x => TemplateDto.FromDomain(x, resources)).ToArray()
        };

        return result.CreateLinks(resources);
    }

    private TemplatesDto CreateLinks(Resources resources)
    {
        AddSelfLink(resources.Url<TemplatesController>(c => nameof(c.GetTemplates)));

        return this;
    }
}
