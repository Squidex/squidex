// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.Commands;

public sealed class MigrateContent : ContentCommand
{
    public ContentData? Data { get; set; }

    public ContentData? NewData { get; set; }
}
