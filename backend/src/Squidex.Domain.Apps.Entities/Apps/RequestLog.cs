// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public struct RequestLog
    {
        public Instant Timestamp;

        public string? RequestMethod;

        public string? RequestPath;

        public string? UserId;

        public string? UserClientId;

        public long ElapsedMs;

        public long Bytes;

        public double Costs;
    }
}
