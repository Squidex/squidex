// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public sealed class AtlasOptions
{
    public string GroupId { get; set; }

    public string ClusterName { get; set; }

    public string PublicKey { get; set; }

    public string PrivateKey { get; set; }

    public bool FullTextEnabled { get; set; }

    public bool IsConfigured()
    {
        return
            !string.IsNullOrWhiteSpace(GroupId) &&
            !string.IsNullOrWhiteSpace(ClusterName) &&
            !string.IsNullOrWhiteSpace(PublicKey) &&
            !string.IsNullOrWhiteSpace(PrivateKey);
    }
}
