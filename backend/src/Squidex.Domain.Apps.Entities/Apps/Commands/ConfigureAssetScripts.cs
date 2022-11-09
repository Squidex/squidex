// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;

namespace Squidex.Domain.Apps.Entities.Apps.Commands;

public sealed class ConfigureAssetScripts : AppCommand
{
    public AssetScripts? Scripts { get; set; }
}
