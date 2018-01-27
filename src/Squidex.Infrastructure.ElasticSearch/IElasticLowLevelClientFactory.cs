// ==========================================================================
//  IElasticLowLevelClientFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Elasticsearch.Net;

namespace Squidex.Infrastructure.ElasticSearch
{
    public interface IElasticLowLevelClientFactory
    {
        IElasticLowLevelClient Create(Uri hostUrl);
    }
}