// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Infrastructure.Log
{
    public interface IArrayWriter
    {
        IArrayWriter WriteValue(string value);

        IArrayWriter WriteValue(double value);

        IArrayWriter WriteValue(long value);

        IArrayWriter WriteValue(bool value);

        IArrayWriter WriteValue(TimeSpan value);

        IArrayWriter WriteValue(Instant value);

        IArrayWriter WriteObject(Action<IObjectWriter> objectWriter);

        IArrayWriter WriteObject<T>(T context, Action<T, IObjectWriter> objectWriter);
    }
}