// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.EventSourcing
{
    public class EventData
    {
        public JToken Payload { get; set; }

        public JToken Metadata { get; set; }

        public string Type { get; set; }
    }
}