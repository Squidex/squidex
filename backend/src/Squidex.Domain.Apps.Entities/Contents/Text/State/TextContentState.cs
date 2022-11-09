// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State;

public sealed class TextContentState
{
    public DomainId AppId { get; set; }

    public DomainId UniqueContentId { get; set; }

    public string DocIdCurrent { get; set; }

    public string? DocIdNew { get; set; }

    public string? DocIdForPublished { get; set; }

    public bool IsDeleted { get; set; }

    public void GenerateDocIdNew()
    {
        if (DocIdCurrent?.EndsWith("_2", StringComparison.Ordinal) != false)
        {
            DocIdNew = $"{UniqueContentId}_1";
        }
        else
        {
            DocIdNew = $"{UniqueContentId}_2";
        }
    }

    public void GenerateDocIdCurrent()
    {
        if (DocIdNew?.EndsWith("_2", StringComparison.Ordinal) != false)
        {
            DocIdCurrent = $"{UniqueContentId}_1";
        }
        else
        {
            DocIdCurrent = $"{UniqueContentId}_2";
        }
    }
}
