// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.News.Models;

public class FeaturesDto
{
    /// <summary>
    /// The latest features.
    /// </summary>
    [LocalizedRequired]
    public List<FeatureDto> Features { get; } = new List<FeatureDto>();

    /// <summary>
    /// The recent version.
    /// </summary>
    public int Version { get; set; }
}
