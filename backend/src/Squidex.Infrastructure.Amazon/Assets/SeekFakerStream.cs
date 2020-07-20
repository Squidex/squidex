// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;

namespace Squidex.Infrastructure.Assets
{
    public sealed class SeekFakerStream : Stream
    {
        private readonly Stream inner;

        public override bool CanRead
        {
            get { return inner.CanRead; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return inner.Length; }
        }

        public override long Position
        {
            get { return inner.Position; }
            set { throw new NotSupportedException(); }
        }

        public SeekFakerStream(Stream inner)
        {
            Guard.NotNull(inner, nameof(inner));

            if (!inner.CanRead)
            {
                throw new ArgumentException("Inner stream must be readable.");
            }

            this.inner = inner;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return inner.Read(buffer, offset, count);
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset != 0 || origin != SeekOrigin.Begin)
            {
                throw new NotSupportedException();
            }

            return 0;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
