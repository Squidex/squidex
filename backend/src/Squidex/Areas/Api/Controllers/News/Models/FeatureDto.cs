// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.News.Models;

public sealed class FeatureDto
{
    /// <summary>
    /// The name of the feature.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description text.
    /// </summary>
    public string Text { get; set; }
}
