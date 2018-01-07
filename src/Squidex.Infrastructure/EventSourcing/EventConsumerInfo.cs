// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class EventConsumerInfo
    {
        public bool IsStopped { get; set; }

        public string Name { get; set; }

        public string Error { get; set; }

        public string Position { get; set; }
    }
}
