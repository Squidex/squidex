// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Templates.Models;

public class TemplateDetailsDto : Resource
{
    /// <summary>
    /// The details of the template.
    /// </summary>
    [LocalizedRequired]
    public string Details { get; set; }

    public static TemplateDetailsDto FromDomain(string name, string details, Resources resources)
    {
        var result = new TemplateDetailsDto
        {
            Details = details
        };

        return result.CreateLinks(name, resources);
    }

    private TemplateDetailsDto CreateLinks(string name, Resources resources)
    {
        var values = new { name };

        AddSelfLink(resources.Url<TemplatesController>(c => nameof(c.GetTemplate), values));

        return this;
    }
}
