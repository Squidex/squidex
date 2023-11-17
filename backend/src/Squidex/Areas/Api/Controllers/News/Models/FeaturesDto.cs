// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.News.Models;

public class FeaturesDto
{
    /// <summary>
    /// The latest features.
    /// </summary>
    public List<FeatureDto> Features { get; } = [];

    /// <summary>
    /// The recent version.
    /// </summary>
    public int Version { get; set; }
}
