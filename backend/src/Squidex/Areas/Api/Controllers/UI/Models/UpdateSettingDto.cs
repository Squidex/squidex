// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Areas.Api.Controllers.UI.Models;

public sealed class UpdateSettingDto
{
    /// <summary>
    /// The value for the setting.
    /// </summary>
    public JsonValue Value { get; set; }
}
