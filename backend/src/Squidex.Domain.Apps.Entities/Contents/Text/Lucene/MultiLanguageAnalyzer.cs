// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Lucene
{
    public sealed class MultiLanguageAnalyzer : AnalyzerWrapper
    {
        private readonly StandardAnalyzer fallbackAnalyzer;
        private readonly Dictionary<string, Analyzer> analyzers = new Dictionary<string, Analyzer>(StringComparer.OrdinalIgnoreCase);

        public MultiLanguageAnalyzer(LuceneVersion version)
            : base(PER_FIELD_REUSE_STRATEGY)
        {
            fallbackAnalyzer = new StandardAnalyzer(version);

            foreach (var type in typeof(StandardAnalyzer).Assembly.GetTypes())
            {
                if (typeof(Analyzer).IsAssignableFrom(type))
                {
                    var language = type.Namespace!.Split('.').Last();

                    if (language.Length == 2)
                    {
                        try
                        {
                            var analyzer = Activator.CreateInstance(type, version)!;

                            analyzers[language] = (Analyzer)analyzer;
                        }
                        catch (MissingMethodException)
                        {
                            continue;
                        }
                    }
                }
            }
        }

        protected override Analyzer GetWrappedAnalyzer(string fieldName)
        {
            if (fieldName.Length > 0)
            {
                var analyzer = analyzers.GetOrDefault(fieldName.Substring(0, 2)) ?? fallbackAnalyzer;

                return analyzer;
            }
            else
            {
                return fallbackAnalyzer;
            }
        }
    }
}
