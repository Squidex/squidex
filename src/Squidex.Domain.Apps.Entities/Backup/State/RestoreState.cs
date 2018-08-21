// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Entities.Backup.State
{
    public class RestoreState
    {
        [JsonProperty]
        public RestoreStateJob Job { get; set; }
    }
}
