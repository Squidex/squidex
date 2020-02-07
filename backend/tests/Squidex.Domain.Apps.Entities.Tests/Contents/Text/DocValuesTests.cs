// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class DocValuesTests
    {
        [Fact]
        public void Should_read_and_write_doc_values()
        {
            var version = LuceneVersion.LUCENE_48;

            var indexWriter =
                new IndexWriter(new RAMDirectory(),
                    new IndexWriterConfig(version, new StandardAnalyzer(version)));

            using (indexWriter)
            {
                for (byte i = 0; i < 255; i++)
                {
                    var document = new Document();

                    document.AddBinaryDocValuesField("field", new BytesRef(new[] { i }));

                    indexWriter.AddDocument(document);
                }

                indexWriter.Commit();

                using (var reader = indexWriter.GetReader(true))
                {
                    var bytesRef = new BytesRef(1);

                    for (byte i = 0; i < 255; i++)
                    {
                        reader.GetBinaryValue("field", i, bytesRef);

                        Assert.Equal(i, bytesRef.Bytes[0]);
                    }
                }
            }
        }
    }
}
