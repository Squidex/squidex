// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class StatusInfo
    {
        public Status Status { get; }

        public string Color { get; }

        public StatusInfo(Status status, string color)
        {
            Status = status;

            Color = color;
        }
    }
}
