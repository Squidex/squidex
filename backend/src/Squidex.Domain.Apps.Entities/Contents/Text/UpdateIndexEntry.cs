// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public class UpdateIndexEntry : IndexCommand
{
    public bool ServeAll { get; set; }

    public bool ServePublished { get; set; }
}
