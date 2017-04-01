// ==========================================================================
//  IArrayWriter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Log
{
    public interface IArrayWriter
    {
        IArrayWriter WriteValue(string value);
        IArrayWriter WriteValue(double value);
        IArrayWriter WriteValue(long value);
        IArrayWriter WriteValue(bool value);

        IArrayWriter WriteValue(TimeSpan value);
        IArrayWriter WriteValue(DateTime value);
        IArrayWriter WriteValue(DateTimeOffset value);

        IArrayWriter WriteObject(string property, Action<IObjectWriter> objectWriter);
    }
}