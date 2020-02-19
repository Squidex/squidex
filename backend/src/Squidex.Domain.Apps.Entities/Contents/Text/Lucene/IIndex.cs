// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Lucene
{
    public interface IIndex
    {
        Analyzer? Analyzer { get; }

        IndexReader? Reader { get; }

        IndexSearcher? Searcher { get; }

        IndexWriter Writer { get; }

        void EnsureReader();

        void MarkStale();
    }
}