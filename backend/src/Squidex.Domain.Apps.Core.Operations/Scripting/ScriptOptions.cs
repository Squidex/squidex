// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Scripting
{
    public struct ScriptOptions
    {
        public bool CanReject { get; set; }

        public bool CanDisallow { get; set; }

        public bool AsContext { get; set; }

        public override readonly string ToString()
        {
            return $"CanReject={CanReject}, CanDisallow={CanDisallow}, AsContext={AsContext}";
        }
    }
}
