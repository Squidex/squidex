// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Orleans.Serialization;

namespace Squidex.Infrastructure.Orleans
{
    internal sealed class StreamWriterWrapper : Stream
    {
        private readonly IBinaryTokenStreamWriter writer;

        public override bool CanRead
        {
            get => false;
        }

        public override bool CanSeek
        {
            get => false;
        }

        public override bool CanWrite
        {
            get => true;
        }

        public override long Length
        {
            get => writer.CurrentOffset;
        }

        public override long Position
        {
            get
            {
                return writer.CurrentOffset;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public StreamWriterWrapper(IBinaryTokenStreamWriter writer)
        {
            this.writer = writer;
        }

        public override void Flush()
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            writer.Write(buffer, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}
