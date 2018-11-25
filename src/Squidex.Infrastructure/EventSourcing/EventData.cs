// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public class EventData
    {
        public string Payload { get; set; }

        public string Metadata { get; set; }

        public string Type { get; set; }
    }
}