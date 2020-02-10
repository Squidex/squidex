// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State
{
    public sealed class TextContentState
    {
        public Guid ContentId { get; set; }

        public string DocIdCurrent { get; set; }

        public string? DocIdNew { get; set; }

        public string? DocIdForPublished { get; set; }

        public void GenerateDocIdNew()
        {
            if (DocIdCurrent?.EndsWith("_2") != false)
            {
                DocIdNew = $"{ContentId}_1";
            }
            else
            {
                DocIdNew = $"{ContentId}_2";
            }
        }

        public void GenerateDocIdCurrent()
        {
            if (DocIdNew?.EndsWith("_2") != false)
            {
                DocIdCurrent = $"{ContentId}_1";
            }
            else
            {
                DocIdCurrent = $"{ContentId}_2";
            }
        }
    }
}
