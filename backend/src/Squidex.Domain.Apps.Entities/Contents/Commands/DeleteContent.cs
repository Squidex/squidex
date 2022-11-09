// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Commands;

public sealed class DeleteContent : ContentCommand
{
    public bool CheckReferrers { get; set; }

    public bool Permanent { get; set; }
}
