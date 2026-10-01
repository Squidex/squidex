// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Text.State;

public sealed class TextContentState
{
    public UniqueContentId UniqueContentId { get; set; }

    public TextState State { get; set; }

    public long? Version { get; set; }
}
