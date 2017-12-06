// ==========================================================================
//  ContentState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.State
{
    public sealed class ContentState : DomainObjectState<ContentState>
    {
        [JsonProperty]
        public IdContentData Data { get; set; }

        [JsonProperty]
        public string Status { get; set; }
    }
}
